using CrmSaas.Application.DTOs;
using CrmSaas.Application.Abstractions;
using CrmSaas.Api.Services;
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
        if (dto.QuoteId.HasValue && !await db.Cotizaciones.AnyAsync(x => x.Id == dto.QuoteId.Value, cancellationToken)) throw new KeyNotFoundException("Cotizacion no encontrada.");
        if (dto.DealId.HasValue && !await db.Negocios.AnyAsync(x => x.Id == dto.DealId.Value, cancellationToken)) throw new KeyNotFoundException("Negocio no encontrado.");

        var entity = new SolicitudCredito
        {
            Numero = $"SOL-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ClienteId = customer.Id,
            ProductoId = product.Id,
            CotizacionId = dto.QuoteId,
            NegocioId = dto.DealId,
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
            Estado = dto.Status,
            Observaciones = dto.Notes
        };

        foreach (var document in DefaultDocuments())
        {
            entity.Documentos.Add(document);
        }

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
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        if (!await db.Clientes.AnyAsync(x => x.Id == dto.CustomerId, cancellationToken)) throw new KeyNotFoundException("Cliente no encontrado.");
        var product = await db.Productos.FirstOrDefaultAsync(x => x.Id == dto.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");

        entity.ClienteId = dto.CustomerId;
        entity.ProductoId = dto.ProductId;
        entity.CotizacionId = dto.QuoteId;
        entity.NegocioId = dto.DealId;
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
        entity.Estado = dto.Status;
        entity.Observaciones = dto.Notes;

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
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

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
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");

        ValidateDecision(entity, dto.Status);
        entity.Estado = dto.Status;
        ApplyDecision(entity, dto.Status, dto.Notes);
        await SyncPipelineAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(entity));
    }

    [HttpPut("{id:guid}/documents/{documentId:guid}")]
    public async Task<ActionResult<CreditApplicationDto>> UpdateDocument(Guid id, Guid documentId, UpsertCreditDocumentDto dto, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
            .Include(x => x.Documentos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Solicitud de credito no encontrada.");
        var document = entity.Documentos.FirstOrDefault(x => x.Id == documentId)
            ?? throw new KeyNotFoundException("Documento no encontrado.");

        document.Tipo = dto.Type;
        document.Nombre = string.IsNullOrWhiteSpace(dto.Name) ? document.Nombre : dto.Name.Trim();
        document.Estado = dto.Status;
        document.FechaRecepcion = dto.Status is EstadoDocumentoCredito.Recibido or EstadoDocumentoCredito.Validado
            ? dto.ReceivedAt ?? DateTime.UtcNow
            : dto.ReceivedAt;
        document.Observaciones = dto.Notes;

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
        document.FechaCarga = DateTime.UtcNow;
        document.Estado = EstadoDocumentoCredito.Recibido;
        document.FechaRecepcion = DateTime.UtcNow;

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

    [HttpGet("{id:guid}/pdf/{template}")]
    public async Task<IActionResult> DownloadTemplate(Guid id, string template, CancellationToken cancellationToken)
    {
        var entity = await db.SolicitudesCredito
            .Include(x => x.Cliente)
            .Include(x => x.Producto)
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
    }

    private static IReadOnlyCollection<DocumentoSolicitudCredito> DefaultDocuments() =>
    [
        new() { Tipo = TipoDocumentoCredito.Cedula, Nombre = "Cedula" },
        new() { Tipo = TipoDocumentoCredito.SoporteIngresos, Nombre = "Soporte de ingresos" },
        new() { Tipo = TipoDocumentoCredito.ReciboServicio, Nombre = "Recibo de servicio o direccion" },
        new() { Tipo = TipoDocumentoCredito.Referencias, Nombre = "Referencias" }
    ];

    private static void MarkReadyIfDocumentsComplete(SolicitudCredito entity)
    {
        if (entity.Documentos.Count > 0 && entity.Documentos.All(x => x.Estado is EstadoDocumentoCredito.Recibido or EstadoDocumentoCredito.Validado))
        {
            entity.Estado = EstadoSolicitudCredito.DocumentosRecibidos;
        }
    }

    private static void ValidateDecision(SolicitudCredito entity, EstadoSolicitudCredito status)
    {
        if (status == EstadoSolicitudCredito.EnEstudio && entity.Documentos.Any(x => x.Estado is EstadoDocumentoCredito.Pendiente or EstadoDocumentoCredito.Rechazado))
        {
            throw new ValidationException("Para enviar a estudio todos los documentos deben estar recibidos o validados.");
        }

        if (status == EstadoSolicitudCredito.Aprobada && entity.Estado != EstadoSolicitudCredito.EnEstudio)
        {
            throw new ValidationException("Solo se puede aprobar una solicitud que este en estudio.");
        }

        if (status == EstadoSolicitudCredito.Desembolsada && entity.Estado != EstadoSolicitudCredito.Aprobada)
        {
            throw new ValidationException("Solo se puede desembolsar una solicitud aprobada.");
        }
    }

    private void ApplyDecision(SolicitudCredito entity, EstadoSolicitudCredito status, string? notes)
    {
        var now = DateTime.UtcNow;
        entity.UsuarioDecision = tenantContext.UsuarioActual;
        entity.ObservacionDecision = string.IsNullOrWhiteSpace(notes) ? entity.ObservacionDecision : notes.Trim();

        switch (status)
        {
            case EstadoSolicitudCredito.DocumentosPendientes:
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
            EstadoSolicitudCredito.DocumentosPendientes => "Preaprobacion",
            EstadoSolicitudCredito.DocumentosRecibidos => "Documentos recibidos",
            EstadoSolicitudCredito.EnEstudio => "Estudio de credito",
            EstadoSolicitudCredito.Aprobada => "Aprobado",
            EstadoSolicitudCredito.Rechazada => "Perdido",
            EstadoSolicitudCredito.Desembolsada => "Entregada",
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
            EstadoSolicitudCredito.Rechazada => EstadoNegocio.Perdido,
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
            x.ClienteId,
            customerName,
            x.ProductoId,
            productName,
            x.CotizacionId,
            x.NegocioId,
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
            x.Estado,
            x.Observaciones,
            x.FechaEnvio,
            x.FechaInicioEstudio,
            x.FechaAprobacion,
            x.FechaRechazo,
            x.FechaDesembolso,
            x.UsuarioDecision,
            x.ObservacionDecision,
            x.Documentos.OrderBy(d => d.Tipo).Select(ToDocumentDto).ToList());
    }

    private static CreditDocumentDto ToDocumentDto(DocumentoSolicitudCredito d) =>
        new(
            d.Id,
            d.Tipo,
            d.Nombre,
            d.Estado,
            d.FechaRecepcion,
            d.Observaciones,
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
