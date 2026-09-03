using CrmSaas.Application.DTOs;
using CrmSaas.Application.Abstractions;
using CrmSaas.Api.Services;
using CrmSaas.Domain.Common;
using CrmSaas.Domain.Entities;
using CrmSaas.Domain.Enums;
using CrmSaas.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/credit-applications")]
public sealed class CreditApplicationsController(CrmDbContext db, IWebHostEnvironment env, ITenantContext tenantContext) : ControllerBase
{
    private static readonly HashSet<string> AllowedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CreditApplicationDto>>> Get(CancellationToken cancellationToken)
    {
        var rows = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpGet("board")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<IReadOnlyCollection<CreditApplicationDto>>> GetBoard(CancellationToken cancellationToken)
    {
        var rows = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .OrderByDescending(x => x.FechaCreacion)
            .ToListAsync(cancellationToken);

        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CreditApplicationDto>> Create(UpsertCreditApplicationDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var customer = await db.Clientes.FirstOrDefaultAsync(x => x.Id == dto.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente no encontrado.");
        var product = await db.Productos.FirstOrDefaultAsync(x => x.Id == dto.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");
        var quote = dto.QuoteId.HasValue
            ? await db.Cotizaciones.FirstOrDefaultAsync(x => x.Id == dto.QuoteId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Cotizacion no encontrada.")
            : null;
        if (dto.DealId.HasValue && !await db.Negocios.AnyAsync(x => x.Id == dto.DealId.Value, cancellationToken)) throw new KeyNotFoundException("Negocio no encontrado.");
        var requirementProfile = await ResolveRequirementProfileAsync(dto.RequirementProfileId ?? quote?.PerfilRequisitoId, cancellationToken);

        var entity = new SolicitudCredito
        {
            Numero = $"SOL-{ColombiaTime.Now:yyyyMMddHHmmss}",
            ClienteId = customer.Id,
            ProductoId = product.Id,
            CotizacionId = dto.QuoteId,
            NegocioId = dto.DealId,
            PerfilRequisitoId = requirementProfile?.Id,
            PerfilRequisito = requirementProfile,
            TipoIdentificacion = dto.IdentificationType,
            NumeroIdentificacion = dto.IdentificationNumber.Trim(),
            FechaNacimiento = dto.BirthDate,
            Celular = dto.Mobile.Trim(),
            Direccion = dto.Address,
            Ciudad = dto.City,
            Ocupacion = dto.Occupation,
            IngresosMensuales = dto.MonthlyIncome,
            CuotaInicial = dto.DownPayment,
            PlazoMeses = dto.TermMonths,
            ValorMoto = dto.MotorcycleValue > 0 ? dto.MotorcycleValue : product.Precio,
            CodeudorNombre = Normalize(dto.CoDebtorName),
            CodeudorIdentificacion = Normalize(dto.CoDebtorIdentification),
            CodeudorCelular = Normalize(dto.CoDebtorMobile),
            CodeudorParentesco = Normalize(dto.CoDebtorRelationship),
            CodeudorIngresosMensuales = dto.CoDebtorMonthlyIncome,
            Referencia1Nombre = Normalize(dto.Reference1Name),
            Referencia1Celular = Normalize(dto.Reference1Mobile),
            Referencia1Relacion = Normalize(dto.Reference1Relationship),
            Referencia2Nombre = Normalize(dto.Reference2Name),
            Referencia2Celular = Normalize(dto.Reference2Mobile),
            Referencia2Relacion = Normalize(dto.Reference2Relationship),
            CodeudorReferencia1Nombre = Normalize(dto.CoDebtorReference1Name),
            CodeudorReferencia1Celular = Normalize(dto.CoDebtorReference1Mobile),
            CodeudorReferencia1Relacion = Normalize(dto.CoDebtorReference1Relationship),
            CodeudorReferencia2Nombre = Normalize(dto.CoDebtorReference2Name),
            CodeudorReferencia2Celular = Normalize(dto.CoDebtorReference2Mobile),
            CodeudorReferencia2Relacion = Normalize(dto.CoDebtorReference2Relationship),
            Estado = dto.Status,
            Observaciones = dto.Notes
        };

        AddChecklistDocuments(entity, requirementProfile);

        db.SolicitudesCredito.Add(entity);
        await SyncPipelineAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        entity.Cliente = customer;
        entity.Producto = product;
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CreditApplicationDto>> Update(Guid id, UpsertCreditApplicationDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        if (!await db.Clientes.AnyAsync(x => x.Id == dto.CustomerId, cancellationToken)) throw new KeyNotFoundException("Cliente no encontrado.");
        var product = await db.Productos.FirstOrDefaultAsync(x => x.Id == dto.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");
        var quote = dto.QuoteId.HasValue
            ? await db.Cotizaciones.FirstOrDefaultAsync(x => x.Id == dto.QuoteId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Cotizacion no encontrada.")
            : null;
        var requirementProfile = await ResolveRequirementProfileAsync(dto.RequirementProfileId ?? quote?.PerfilRequisitoId, cancellationToken);

        entity.ClienteId = dto.CustomerId;
        entity.ProductoId = dto.ProductId;
        entity.CotizacionId = dto.QuoteId;
        entity.NegocioId = dto.DealId;
        entity.PerfilRequisitoId = requirementProfile?.Id;
        entity.PerfilRequisito = requirementProfile;
        entity.TipoIdentificacion = dto.IdentificationType;
        entity.NumeroIdentificacion = dto.IdentificationNumber.Trim();
        entity.FechaNacimiento = dto.BirthDate;
        entity.Celular = dto.Mobile.Trim();
        entity.Direccion = dto.Address;
        entity.Ciudad = dto.City;
        entity.Ocupacion = dto.Occupation;
        entity.IngresosMensuales = dto.MonthlyIncome;
        entity.CuotaInicial = dto.DownPayment;
        entity.PlazoMeses = dto.TermMonths;
        entity.ValorMoto = dto.MotorcycleValue > 0 ? dto.MotorcycleValue : product.Precio;
        entity.CodeudorNombre = Normalize(dto.CoDebtorName);
        entity.CodeudorIdentificacion = Normalize(dto.CoDebtorIdentification);
        entity.CodeudorCelular = Normalize(dto.CoDebtorMobile);
        entity.CodeudorParentesco = Normalize(dto.CoDebtorRelationship);
        entity.CodeudorIngresosMensuales = dto.CoDebtorMonthlyIncome;
        entity.Referencia1Nombre = Normalize(dto.Reference1Name);
        entity.Referencia1Celular = Normalize(dto.Reference1Mobile);
        entity.Referencia1Relacion = Normalize(dto.Reference1Relationship);
        entity.Referencia2Nombre = Normalize(dto.Reference2Name);
        entity.Referencia2Celular = Normalize(dto.Reference2Mobile);
        entity.Referencia2Relacion = Normalize(dto.Reference2Relationship);
        entity.CodeudorReferencia1Nombre = Normalize(dto.CoDebtorReference1Name);
        entity.CodeudorReferencia1Celular = Normalize(dto.CoDebtorReference1Mobile);
        entity.CodeudorReferencia1Relacion = Normalize(dto.CoDebtorReference1Relationship);
        entity.CodeudorReferencia2Nombre = Normalize(dto.CoDebtorReference2Name);
        entity.CodeudorReferencia2Celular = Normalize(dto.CoDebtorReference2Mobile);
        entity.CodeudorReferencia2Relacion = Normalize(dto.CoDebtorReference2Relationship);
        entity.Estado = dto.Status;
        entity.Observaciones = dto.Notes;
        AddMissingChecklistDocuments(entity, requirementProfile);

        await SyncPipelineAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        entity.Producto = product;
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/status")]
    public async Task<ActionResult<CreditApplicationDto>> ChangeStatus(Guid id, ChangeCreditApplicationStatusDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        if (dto.Status is EstadoSolicitudCredito.EnEstudio or EstadoSolicitudCredito.Aprobada or EstadoSolicitudCredito.Rechazada or EstadoSolicitudCredito.Desembolsada && !CanValidateDocuments())
        {
            return Forbid();
        }

        ValidateDecision(entity, dto.Status);
        entity.Estado = dto.Status;
        ApplyDecision(entity, dto.Status, null);
        await SyncPipelineAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/decision")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<CreditApplicationDto>> Decide(Guid id, CreditApplicationDecisionDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        ValidateDecision(entity, dto.Status);
        ApplyFormalStudy(entity, dto);
        entity.Estado = dto.Status;
        ApplyDecision(entity, dto.Status, dto.Notes);
        await SyncPipelineAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/study/step0")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<CreditApplicationDto>> SaveStep0(Guid id, CreditStudyStep0Dto dto, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        entity.RuntConsultado = dto.RuntChecked;
        entity.SimitConsultado = dto.SimitChecked;
        entity.IdentidadValidada = dto.IdentityValidated;
        entity.ObservacionPaso0 = Normalize(dto.Notes);
        entity.UsuarioPaso0 = tenantContext.UsuarioActual;
        entity.FechaRevisionPaso0 = ColombiaTime.Now;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/study/recalculation")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<CreditApplicationDto>> SaveRecalculation(Guid id, CreditStudyRecalculationDto dto, CancellationToken cancellationToken)
    {
        if (dto.ApprovedAmount <= 0) throw new ValidationException("El valor aprobado debe ser mayor a cero.");
        if (dto.ApprovedDownPayment < 0) throw new ValidationException("La cuota inicial aprobada no puede ser negativa.");
        if (dto.ApprovedTermMonths <= 0) throw new ValidationException("El plazo aprobado debe ser mayor a cero.");
        if (dto.ApprovedMonthlyPayment <= 0) throw new ValidationException("La cuota aprobada debe ser mayor a cero.");

        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        entity.ValorAprobadoAnalista = dto.ApprovedAmount;
        entity.CuotaInicialAprobada = dto.ApprovedDownPayment;
        entity.PlazoAprobadoMeses = dto.ApprovedTermMonths;
        entity.CuotaMensualAprobada = dto.ApprovedMonthlyPayment;
        entity.ObservacionDecision = string.IsNullOrWhiteSpace(dto.Notes) ? entity.ObservacionDecision : dto.Notes.Trim();
        entity.UsuarioDecision = tenantContext.UsuarioActual;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}/documents/{documentId:guid}")]
    public async Task<ActionResult<CreditApplicationDto>> UpdateDocument(Guid id, Guid documentId, UpsertCreditDocumentDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");
        var document = entity.Documentos.FirstOrDefault(x => x.Id == documentId)
            ?? throw new KeyNotFoundException("Documento no encontrado.");

        document.Tipo = dto.Type;
        document.Nombre = string.IsNullOrWhiteSpace(dto.Name) ? document.Nombre : dto.Name.Trim();
        document.ClienteId = entity.ClienteId;
        document.FechaVencimiento = dto.ExpiresAt;
        if (dto.Status is EstadoDocumentoCredito.Validado or EstadoDocumentoCredito.Rechazado && !CanValidateDocuments())
        {
            return Forbid();
        }

        if (dto.Status == EstadoDocumentoCredito.Rechazado && string.IsNullOrWhiteSpace(dto.RejectionReason))
        {
            throw new ValidationException("Debe registrar el motivo de rechazo del documento.");
        }

        document.Estado = dto.Status;
        document.FechaRecepcion = dto.Status is EstadoDocumentoCredito.Recibido or EstadoDocumentoCredito.Validado
            ? dto.ReceivedAt ?? ColombiaTime.Now
            : dto.ReceivedAt;
        document.Observaciones = dto.Notes;
        ApplyDocumentAudit(document, dto.Status, dto.RejectionReason);

        MarkReadyIfDocumentsComplete(entity);

        await SyncPipelineAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/documents/{documentId:guid}/file")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<ActionResult<CreditApplicationDto>> UploadDocument(Guid id, Guid documentId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) throw new ValidationException("Debe seleccionar un archivo.");
        if (file.Length > MaxFileSizeBytes) throw new ValidationException("El archivo no puede superar 10 MB.");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedFileExtensions.Contains(extension))
        {
            throw new ValidationException("Solo se permiten archivos PDF o imagenes JPG, PNG y WEBP.");
        }

        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");
        var document = entity.Documentos.FirstOrDefault(x => x.Id == documentId)
            ?? throw new KeyNotFoundException("Documento no encontrado.");

        var originalName = Path.GetFileName(file.FileName);
        var folder = Path.Combine(StorageRoot, "credit-documents", entity.EmpresaId.ToString("N"), entity.Id.ToString("N"));
        Directory.CreateDirectory(folder);
        var storedName = $"{document.Id:N}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(folder, storedName);

        await using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        DeleteStoredFile(document.RutaArchivo);

        document.NombreArchivo = originalName;
        document.RutaArchivo = path;
        document.ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType;
        document.TamanoBytes = file.Length;
        document.FechaCarga = ColombiaTime.Now;
        document.Estado = EstadoDocumentoCredito.Recibido;
        document.FechaRecepcion = ColombiaTime.Now;
        document.ClienteId = entity.ClienteId;
        document.FechaVencimiento ??= DefaultExpiration(document.Tipo, ColombiaTime.Now);
        ApplyDocumentAudit(document, EstadoDocumentoCredito.Recibido, null);

        MarkReadyIfDocumentsComplete(entity);
        await SyncPipelineAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpGet("{id:guid}/documents/{documentId:guid}/file")]
    public async Task<IActionResult> DownloadDocument(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        var document = await db.DocumentosSolicitudCredito
            .Where(x => x.SolicitudCreditoId == id && x.Id == documentId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Documento no encontrado.");

        if (string.IsNullOrWhiteSpace(document.RutaArchivo) || !IsStoredPath(document.RutaArchivo) || !System.IO.File.Exists(document.RutaArchivo))
        {
            throw new KeyNotFoundException("El archivo no esta disponible en el servidor.");
        }

        return PhysicalFile(
            document.RutaArchivo,
            string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType,
            string.IsNullOrWhiteSpace(document.NombreArchivo) ? document.Nombre : document.NombreArchivo);
    }

    [HttpDelete("{id:guid}/documents/{documentId:guid}/file")]
    public async Task<ActionResult<CreditApplicationDto>> DeleteDocumentFile(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");
        var document = entity.Documentos.FirstOrDefault(x => x.Id == documentId)
            ?? throw new KeyNotFoundException("Documento no encontrado.");
        if (string.IsNullOrWhiteSpace(document.RutaArchivo))
        {
            throw new ValidationException("El documento no tiene un archivo cargado.");
        }

        DeleteStoredFile(document.RutaArchivo);
        document.NombreArchivo = null;
        document.RutaArchivo = null;
        document.ContentType = null;
        document.TamanoBytes = null;
        document.FechaCarga = null;
        document.Estado = EstadoDocumentoCredito.Pendiente;
        document.FechaVencimiento = null;
        ApplyDocumentAudit(document, EstadoDocumentoCredito.Pendiente, null);
        if (entity.Estado == EstadoSolicitudCredito.DocumentosRecibidos)
        {
            entity.Estado = EstadoSolicitudCredito.DocumentosPendientes;
        }

        await SyncPipelineAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/study/credit-bureau")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<CreditApplicationDto>> SaveCreditBureau(Guid id, CreditBureauCheckDto dto, CancellationToken cancellationToken)
    {
        if (dto.ClientScore is < 0 or > 1000) throw new ValidationException("El puntaje del cliente debe estar entre 0 y 1000.");
        if (dto.CoDebtorScore is < 0 or > 1000) throw new ValidationException("El puntaje del codeudor debe estar entre 0 y 1000.");

        var entity = await LoadApplicationAsync(id, cancellationToken);
        if (dto.CoDebtorChecked && string.IsNullOrWhiteSpace(entity.CodeudorNombre))
        {
            throw new ValidationException("La solicitud no tiene un codeudor registrado.");
        }

        entity.DataCreditoClienteConsultado = dto.ClientChecked;
        entity.DataCreditoPuntajeCliente = dto.ClientChecked ? dto.ClientScore : null;
        entity.DataCreditoCodeudorConsultado = dto.CoDebtorChecked;
        entity.DataCreditoPuntajeCodeudor = dto.CoDebtorChecked ? dto.CoDebtorScore : null;
        entity.FechaRevisionDataCredito = dto.ClientChecked && (string.IsNullOrWhiteSpace(entity.CodeudorNombre) || dto.CoDebtorChecked) ? ColombiaTime.Now : null;
        entity.UsuarioDataCredito = tenantContext.UsuarioActual;
        entity.ObservacionDataCredito = Normalize(dto.Notes);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/workflow/signatures")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<CreditApplicationDto>> SaveSignatures(Guid id, CreditWorkflowMilestoneDto dto, CancellationToken cancellationToken)
    {
        var entity = await LoadApplicationAsync(id, cancellationToken);
        if (dto.Completed && entity.Estado is not (EstadoSolicitudCredito.Aprobada or EstadoSolicitudCredito.Desembolsada))
        {
            throw new ValidationException("Las firmas solo pueden completarse después de aprobar el crédito.");
        }

        entity.FirmasCompletas = dto.Completed;
        entity.FechaFirmasCompletas = dto.Completed ? ColombiaTime.Now : null;
        entity.UsuarioFirmas = tenantContext.UsuarioActual;
        entity.ObservacionFirmas = Normalize(dto.Notes);
        if (!dto.Completed) ClearFinalWorkflow(entity);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/workflow/final-review")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<CreditApplicationDto>> SaveFinalReview(Guid id, CreditWorkflowMilestoneDto dto, CancellationToken cancellationToken)
    {
        var entity = await LoadApplicationAsync(id, cancellationToken);
        if (dto.Completed && !entity.FirmasCompletas)
        {
            throw new ValidationException("Debe completar las firmas antes de aprobar la revisión final.");
        }

        entity.RevisionFinalAprobada = dto.Completed;
        entity.FechaRevisionFinal = dto.Completed ? ColombiaTime.Now : null;
        entity.UsuarioRevisionFinal = tenantContext.UsuarioActual;
        entity.ObservacionRevisionFinal = Normalize(dto.Notes);
        if (!dto.Completed) ClearWelcome(entity);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPost("{id:guid}/workflow/welcome")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<CreditApplicationDto>> SaveWelcome(Guid id, CreditWorkflowMilestoneDto dto, CancellationToken cancellationToken)
    {
        var entity = await LoadApplicationAsync(id, cancellationToken);
        if (dto.Completed && !entity.RevisionFinalAprobada)
        {
            throw new ValidationException("Debe aprobar la revisión final antes de completar la bienvenida.");
        }

        entity.BienvenidaCompletada = dto.Completed;
        entity.FechaBienvenida = dto.Completed ? ColombiaTime.Now : null;
        entity.UsuarioBienvenida = tenantContext.UsuarioActual;
        entity.ObservacionBienvenida = Normalize(dto.Notes);

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpGet("{id:guid}/pdf/{template}")]
    public async Task<IActionResult> DownloadTemplate(Guid id, string template, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        var normalized = template.Trim().ToLowerInvariant();
        if (normalized == "carta-aprobacion" && entity.Estado is not (EstadoSolicitudCredito.Aprobada or EstadoSolicitudCredito.Desembolsada))
        {
            throw new ValidationException("La carta de aprobacion solo se puede generar para solicitudes aprobadas o desembolsadas.");
        }

        if (normalized == "orden-entrega" && entity.Estado is not (EstadoSolicitudCredito.Aprobada or EstadoSolicitudCredito.Desembolsada))
        {
            throw new ValidationException("La orden de entrega solo se puede generar para solicitudes aprobadas o desembolsadas.");
        }

        var company = await db.Empresas.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == tenantContext.EmpresaId, cancellationToken);
        var dto = ToDto(entity);
        var bytes = SimplePdfGenerator.CreditApplication(dto, company?.Nombre ?? "Empresa", normalized);
        return File(bytes, "application/pdf", SimplePdfGenerator.CreditTemplateFileName(dto, normalized));
    }

    private static void Validate(UpsertCreditApplicationDto dto)
    {
        if (dto.CustomerId == Guid.Empty) throw new ValidationException("Debe seleccionar un cliente.");
        if (dto.ProductId == Guid.Empty) throw new ValidationException("Debe seleccionar un producto.");
        if (string.IsNullOrWhiteSpace(dto.IdentificationNumber)) throw new ValidationException("El numero de identificacion es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Mobile)) throw new ValidationException("El celular o WhatsApp es obligatorio.");
        if (dto.MonthlyIncome < 0) throw new ValidationException("Los ingresos no pueden ser negativos.");
        if (dto.DownPayment < 0) throw new ValidationException("La cuota inicial no puede ser negativa.");
        if (dto.TermMonths <= 0) throw new ValidationException("El plazo debe ser mayor a cero.");
        if (dto.CoDebtorMonthlyIncome.HasValue && dto.CoDebtorMonthlyIncome < 0) throw new ValidationException("Los ingresos del codeudor no pueden ser negativos.");
        if (!string.IsNullOrWhiteSpace(dto.CoDebtorName) && string.IsNullOrWhiteSpace(dto.CoDebtorMobile)) throw new ValidationException("Si registra codeudor, el celular del codeudor es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Reference1Name)) throw new ValidationException("El nombre de la referencia 1 es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Reference1Mobile)) throw new ValidationException("El celular de la referencia 1 es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Reference1Relationship)) throw new ValidationException("La relacion de la referencia 1 es obligatoria.");
        if (string.IsNullOrWhiteSpace(dto.Reference2Name)) throw new ValidationException("El nombre de la referencia 2 es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Reference2Mobile)) throw new ValidationException("El celular de la referencia 2 es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Reference2Relationship)) throw new ValidationException("La relacion de la referencia 2 es obligatoria.");
        if (!string.IsNullOrWhiteSpace(dto.CoDebtorName))
        {
            if (string.IsNullOrWhiteSpace(dto.CoDebtorReference1Name)) throw new ValidationException("El nombre de la referencia 1 del codeudor es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.CoDebtorReference1Mobile)) throw new ValidationException("El celular de la referencia 1 del codeudor es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.CoDebtorReference1Relationship)) throw new ValidationException("La relacion de la referencia 1 del codeudor es obligatoria.");
            if (string.IsNullOrWhiteSpace(dto.CoDebtorReference2Name)) throw new ValidationException("El nombre de la referencia 2 del codeudor es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.CoDebtorReference2Mobile)) throw new ValidationException("El celular de la referencia 2 del codeudor es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.CoDebtorReference2Relationship)) throw new ValidationException("La relacion de la referencia 2 del codeudor es obligatoria.");
        }
    }

    private async Task<SolicitudCredito> LoadApplicationAsync(Guid id, CancellationToken cancellationToken) =>
        await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.PerfilRequisito)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new KeyNotFoundException("Solicitud de crédito no encontrada.");

    private static void ClearFinalWorkflow(SolicitudCredito entity)
    {
        entity.RevisionFinalAprobada = false;
        entity.FechaRevisionFinal = null;
        entity.UsuarioRevisionFinal = null;
        entity.ObservacionRevisionFinal = null;
        ClearWelcome(entity);
    }

    private static void ClearWelcome(SolicitudCredito entity)
    {
        entity.BienvenidaCompletada = false;
        entity.FechaBienvenida = null;
        entity.UsuarioBienvenida = null;
        entity.ObservacionBienvenida = null;
    }

    private async Task<PerfilRequisito?> ResolveRequirementProfileAsync(Guid? profileId, CancellationToken cancellationToken)
    {
        if (profileId.HasValue)
        {
            return await db.PerfilesRequisito
                .Include(x => x.Documentos)
                .FirstOrDefaultAsync(x => x.Id == profileId.Value && x.Activo, cancellationToken)
                ?? throw new KeyNotFoundException("Perfil de requisitos no encontrado o inactivo.");
        }

        return await db.PerfilesRequisito
            .Include(x => x.Documentos)
            .Where(x => x.Activo)
            .OrderByDescending(x => x.Codigo == "EMPLEADO")
            .ThenBy(x => x.Nombre)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static void AddChecklistDocuments(SolicitudCredito entity, PerfilRequisito? profile)
    {
        var documents = profile?.Documentos.Count > 0
            ? profile.Documentos.OrderBy(x => x.Orden).Select(ToApplicationDocument)
            : DefaultDocuments();

        foreach (var document in documents)
        {
            PrepareDocument(entity, document);
            entity.Documentos.Add(document);
        }
    }

    private static void AddMissingChecklistDocuments(SolicitudCredito entity, PerfilRequisito? profile)
    {
        if (profile?.Documentos.Count is not > 0) return;
        var existingNames = entity.Documentos.Select(x => x.Nombre.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var document in profile.Documentos.OrderBy(x => x.Orden))
        {
            if (existingNames.Contains(document.Nombre.Trim())) continue;
            var applicationDocument = ToApplicationDocument(document);
            PrepareDocument(entity, applicationDocument);
            entity.Documentos.Add(applicationDocument);
        }
    }

    private static DocumentoSolicitudCredito ToApplicationDocument(DocumentoPerfilRequisito document) => new()
    {
        Tipo = document.Tipo,
        Nombre = document.Obligatorio ? document.Nombre : $"{document.Nombre} (opcional)",
        Observaciones = document.Descripcion
    };

    private static IReadOnlyCollection<DocumentoSolicitudCredito> DefaultDocuments() =>
    [
        new() { Tipo = TipoDocumentoCredito.Cedula, Nombre = "Cedula" },
        new() { Tipo = TipoDocumentoCredito.SoporteIngresos, Nombre = "Soporte de ingresos" },
        new() { Tipo = TipoDocumentoCredito.ReciboServicio, Nombre = "Recibo de servicio o direccion" },
        new() { Tipo = TipoDocumentoCredito.Referencias, Nombre = "Referencias" }
    ];

    private static void PrepareDocument(SolicitudCredito entity, DocumentoSolicitudCredito document)
    {
        document.ClienteId = entity.ClienteId;
        document.FechaVencimiento ??= DefaultExpiration(document.Tipo, ColombiaTime.Now);
    }

    private static void MarkReadyIfDocumentsComplete(SolicitudCredito entity)
    {
        if (entity.Documentos.Count > 0 && entity.Documentos.All(x => x.Estado is EstadoDocumentoCredito.Recibido or EstadoDocumentoCredito.Validado))
        {
            if (entity.Estado is EstadoSolicitudCredito.Borrador or EstadoSolicitudCredito.Cotizado or EstadoSolicitudCredito.Interesado or EstadoSolicitudCredito.DocumentosPendientes)
            {
                entity.Estado = EstadoSolicitudCredito.DocumentosRecibidos;
            }
        }
    }

    private bool CanValidateDocuments() =>
        User.IsInRole("Administrador") || User.IsInRole("Supervisor");

    private void ApplyDocumentAudit(DocumentoSolicitudCredito document, EstadoDocumentoCredito status, string? rejectionReason)
    {
        var now = ColombiaTime.Now;
        if (status == EstadoDocumentoCredito.Validado)
        {
            document.FechaValidacion = now;
            document.UsuarioValidacion = tenantContext.UsuarioActual;
            document.FechaRechazo = null;
            document.MotivoRechazo = null;
            document.FechaRecepcion ??= now;
            return;
        }

        if (status == EstadoDocumentoCredito.Rechazado)
        {
            document.FechaRechazo = now;
            document.MotivoRechazo = rejectionReason?.Trim();
            document.FechaValidacion = null;
            document.UsuarioValidacion = null;
            return;
        }

        if (status == EstadoDocumentoCredito.Pendiente)
        {
            document.FechaRecepcion = null;
        }

        document.FechaRechazo = null;
        document.MotivoRechazo = null;
        document.FechaValidacion = null;
        document.UsuarioValidacion = null;
    }

    private static DateTime? DefaultExpiration(TipoDocumentoCredito type, DateTime baseDate) => type switch
    {
        TipoDocumentoCredito.Cedula => baseDate.Date.AddYears(1),
        TipoDocumentoCredito.SoporteIngresos => baseDate.Date.AddDays(30),
        TipoDocumentoCredito.ReciboServicio => baseDate.Date.AddDays(60),
        _ => null
    };

    private static void ValidateDecision(SolicitudCredito entity, EstadoSolicitudCredito status)
    {
        if (status == EstadoSolicitudCredito.EnEstudio && entity.Documentos.Any(x => x.Estado is EstadoDocumentoCredito.Pendiente or EstadoDocumentoCredito.Rechazado))
        {
            throw new ValidationException("Para enviar a estudio todos los documentos deben estar recibidos o validados.");
        }

        if (status == EstadoSolicitudCredito.EnEstudio && (!entity.RuntConsultado || !entity.SimitConsultado || !entity.IdentidadValidada))
        {
            throw new ValidationException("Antes de enviar a estudio debe completar paso 0: RUNT, SIMIT e identidad validada.");
        }

        if (status == EstadoSolicitudCredito.EnEstudio && (!entity.DataCreditoClienteConsultado || (!string.IsNullOrWhiteSpace(entity.CodeudorNombre) && !entity.DataCreditoCodeudorConsultado)))
        {
            throw new ValidationException("Antes de enviar a estudio debe completar la consulta de Datacrédito del cliente y del codeudor cuando aplique.");
        }

        if (status == EstadoSolicitudCredito.Aprobada && entity.Estado != EstadoSolicitudCredito.EnEstudio)
        {
            throw new ValidationException("Solo se puede aprobar una solicitud que este en estudio.");
        }

        if (status == EstadoSolicitudCredito.Desembolsada && entity.Estado != EstadoSolicitudCredito.Aprobada)
        {
            throw new ValidationException("Solo se puede desembolsar una solicitud aprobada.");
        }

        if (status == EstadoSolicitudCredito.Desembolsada && !entity.RevisionFinalAprobada)
        {
            throw new ValidationException("Debe completar las firmas y la revisión final antes de autorizar la entrega.");
        }

        if (status == EstadoSolicitudCredito.Desistida && entity.Estado == EstadoSolicitudCredito.Desembolsada)
        {
            throw new ValidationException("No se puede desistir una solicitud ya entregada.");
        }
    }

    private static void ApplyFormalStudy(SolicitudCredito entity, CreditApplicationDecisionDto dto)
    {
        var result = Normalize(dto.Result);
        if (dto.Status == EstadoSolicitudCredito.Aprobada)
        {
            if (dto.RequiresCoDebtor && string.IsNullOrWhiteSpace(entity.CodeudorNombre))
            {
                throw new ValidationException("Para aprobar con codeudor debe registrar los datos del codeudor.");
            }

            entity.ResultadoEstudio = dto.RequiresCoDebtor ? "Aprobado con codeudor" : "Aprobado";
            if (!string.IsNullOrWhiteSpace(result) && result.Contains("ajuste", StringComparison.OrdinalIgnoreCase))
            {
                entity.ResultadoEstudio = dto.RequiresCoDebtor ? "Aprobado con ajuste y codeudor" : "Aprobado con ajuste";
            }

            entity.ValorAprobadoAnalista = dto.ApprovedAmount ?? entity.ValorAprobadoAnalista ?? entity.ValorMoto;
            entity.CuotaInicialAprobada = dto.ApprovedDownPayment ?? entity.CuotaInicialAprobada ?? entity.CuotaInicial;
            entity.PlazoAprobadoMeses = dto.ApprovedTermMonths ?? entity.PlazoAprobadoMeses ?? entity.PlazoMeses;
            entity.CuotaMensualAprobada = dto.ApprovedMonthlyPayment ?? entity.CuotaMensualAprobada;
            entity.RequiereCodeudorParaAprobar = dto.RequiresCoDebtor;
            entity.CondicionesFinales = Normalize(dto.FinalConditions);
            return;
        }

        if (dto.Status == EstadoSolicitudCredito.Rechazada)
        {
            entity.ResultadoEstudio = string.IsNullOrWhiteSpace(result) ? "Negado" : result;
            entity.CondicionesFinales = Normalize(dto.FinalConditions);
            entity.RequiereCodeudorParaAprobar = false;
        }
    }

    private void ApplyDecision(SolicitudCredito entity, EstadoSolicitudCredito status, string? notes)
    {
        var now = ColombiaTime.Now;
        entity.UsuarioDecision = tenantContext.UsuarioActual;
        entity.ObservacionDecision = string.IsNullOrWhiteSpace(notes) ? entity.ObservacionDecision : notes.Trim();

        switch (status)
        {
            case EstadoSolicitudCredito.DocumentosPendientes:
                entity.FechaEnvio ??= now;
                break;
            case EstadoSolicitudCredito.Interesado:
                entity.FechaEnvio ??= now;
                break;
            case EstadoSolicitudCredito.EnEstudio:
                entity.FechaInicioEstudio ??= now;
                break;
            case EstadoSolicitudCredito.Aprobada:
                entity.FechaAprobacion ??= now;
                entity.FechaRechazo = null;
                break;
            case EstadoSolicitudCredito.Rechazada:
                entity.FechaRechazo ??= now;
                break;
            case EstadoSolicitudCredito.Desembolsada:
                entity.FechaDesembolso ??= now;
                break;
            case EstadoSolicitudCredito.Desistida:
                entity.FechaRechazo ??= now;
                break;
        }
    }

    private string StorageRoot => Path.Combine(env.ContentRootPath, "App_Data", "uploads");

    private bool IsStoredPath(string path)
    {
        var fullRoot = Path.GetFullPath(StorageRoot);
        var fullPath = Path.GetFullPath(path);
        return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    private void DeleteStoredFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !IsStoredPath(path) || !System.IO.File.Exists(path)) return;
        System.IO.File.Delete(path);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task SyncPipelineAsync(SolicitudCredito application, CancellationToken cancellationToken)
    {
        if (!application.NegocioId.HasValue) return;

        var deal = await db.Negocios.FirstOrDefaultAsync(x => x.Id == application.NegocioId.Value, cancellationToken);
        if (deal is null) return;

        var stageName = application.Estado switch
        {
            EstadoSolicitudCredito.Cotizado => "Cotizado",
            EstadoSolicitudCredito.Interesado => "Interesado",
            EstadoSolicitudCredito.DocumentosPendientes => "Documentos pendientes",
            EstadoSolicitudCredito.DocumentosRecibidos => "Credito en estudio",
            EstadoSolicitudCredito.EnEstudio => "Credito en estudio",
            EstadoSolicitudCredito.Aprobada => "Aprobado",
            EstadoSolicitudCredito.Rechazada => "Rechazado",
            EstadoSolicitudCredito.Desembolsada => "Entregado",
            EstadoSolicitudCredito.Desistida => "Desistido",
            _ => null
        };

        if (stageName is not null)
        {
            var stage = await db.EtapasNegocio
                .FirstOrDefaultAsync(x => x.Activa && x.Nombre == stageName, cancellationToken);
            if (stage is not null)
            {
                deal.EtapaNegocioId = stage.Id;
                deal.ProbabilidadCierre = stage.ProbabilidadPredeterminada;
            }
        }

        deal.Estado = application.Estado switch
        {
            EstadoSolicitudCredito.Rechazada or EstadoSolicitudCredito.Desistida => EstadoNegocio.Perdido,
            EstadoSolicitudCredito.Desembolsada => EstadoNegocio.Ganado,
            _ => EstadoNegocio.Abierto
        };
    }

    private static CreditApplicationDto ToDto(SolicitudCredito x)
    {
        var customerName = x.Cliente is null ? "Cliente" : $"{x.Cliente.Nombres} {x.Cliente.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(customerName) && x.Cliente is not null) customerName = x.Cliente.Nombre;
        var productName = x.Producto is null ? "Producto" : ProductName(x.Producto);
        return new CreditApplicationDto(
            x.Id,
            x.Numero,
            x.FechaCreacion,
            x.ClienteId,
            customerName,
            x.ProductoId,
            productName,
            x.CotizacionId,
            x.NegocioId,
            x.PerfilRequisitoId,
            x.PerfilRequisito?.Nombre,
            x.TipoIdentificacion,
            x.NumeroIdentificacion,
            x.FechaNacimiento,
            x.Celular,
            x.Direccion,
            x.Ciudad,
            x.Ocupacion,
            x.IngresosMensuales,
            x.CuotaInicial,
            x.PlazoMeses,
            x.ValorMoto,
            x.CodeudorNombre,
            x.CodeudorIdentificacion,
            x.CodeudorCelular,
            x.CodeudorParentesco,
            x.CodeudorIngresosMensuales,
            x.Referencia1Nombre,
            x.Referencia1Celular,
            x.Referencia1Relacion,
            x.Referencia2Nombre,
            x.Referencia2Celular,
            x.Referencia2Relacion,
            x.CodeudorReferencia1Nombre,
            x.CodeudorReferencia1Celular,
            x.CodeudorReferencia1Relacion,
            x.CodeudorReferencia2Nombre,
            x.CodeudorReferencia2Celular,
            x.CodeudorReferencia2Relacion,
            x.Estado,
            x.Observaciones,
            x.FechaEnvio,
            x.FechaInicioEstudio,
            x.FechaRevisionPaso0,
            x.RuntConsultado,
            x.SimitConsultado,
            x.IdentidadValidada,
            x.UsuarioPaso0,
            x.ObservacionPaso0,
            x.DataCreditoClienteConsultado,
            x.DataCreditoPuntajeCliente,
            x.DataCreditoCodeudorConsultado,
            x.DataCreditoPuntajeCodeudor,
            x.FechaRevisionDataCredito,
            x.UsuarioDataCredito,
            x.ObservacionDataCredito,
            x.ValorAprobadoAnalista,
            x.CuotaInicialAprobada,
            x.PlazoAprobadoMeses,
            x.CuotaMensualAprobada,
            x.RequiereCodeudorParaAprobar,
            x.CondicionesFinales,
            x.ResultadoEstudio,
            x.FechaAprobacion,
            x.FechaRechazo,
            x.FechaDesembolso,
            x.UsuarioDecision,
            x.ObservacionDecision,
            x.FirmasCompletas,
            x.FechaFirmasCompletas,
            x.UsuarioFirmas,
            x.ObservacionFirmas,
            x.RevisionFinalAprobada,
            x.FechaRevisionFinal,
            x.UsuarioRevisionFinal,
            x.ObservacionRevisionFinal,
            x.BienvenidaCompletada,
            x.FechaBienvenida,
            x.UsuarioBienvenida,
            x.ObservacionBienvenida,
            x.Documentos.OrderBy(d => d.Tipo).ThenBy(d => d.Nombre).Select(ToDocumentDto).ToList());
    }

    private static CreditDocumentDto ToDocumentDto(DocumentoSolicitudCredito d) =>
        new(
            d.Id,
            d.ClienteId,
            d.Tipo,
            d.Nombre,
            d.Estado,
            d.FechaRecepcion,
            d.FechaVencimiento,
            d.Observaciones,
            d.FechaRechazo,
            d.MotivoRechazo,
            d.FechaValidacion,
            d.UsuarioValidacion,
            d.FechaVencimiento.HasValue && d.FechaVencimiento.Value.Date < ColombiaTime.Now.Date,
            d.FechaVencimiento.HasValue ? (int)(d.FechaVencimiento.Value.Date - ColombiaTime.Now.Date).TotalDays : null,
            !string.IsNullOrWhiteSpace(d.RutaArchivo),
            d.NombreArchivo,
            d.ContentType,
            d.TamanoBytes,
            d.FechaCarga);

    private static string ProductName(Producto product)
    {
        if (!string.IsNullOrWhiteSpace(product.Nombre)) return product.Nombre.Trim();
        return $"{product.Marca} {product.Modelo} {product.Referencia}".Trim();
    }
}
