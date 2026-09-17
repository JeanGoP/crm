using System.Reflection;
using CrmSaas.Application.DTOs;
using CrmSaas.Application.Abstractions;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CrmSaas.Api.Controllers;
using CrmSaas.Api.Services;
using CrmSaas.Domain.Entities;
using CrmSaas.Domain.Enums;
using FluentValidation;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
static void Invoke(string name, params object?[] args) => typeof(CreditApplicationsController)
    .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, args);

var application = new SolicitudCredito
{
    ClienteId = Guid.NewGuid(), RuntConsultado = true, SimitConsultado = true,
    IdentidadValidada = true, DataCreditoClienteConsultado = true
};
var profile = new PerfilRequisito();
profile.Documentos.Add(new DocumentoPerfilRequisito { Nombre = "Referencias personales", Obligatorio = true });
Invoke("AddChecklistDocuments", application, profile);
Check(application.Documentos.Count == 15, "Debe usar el catálogo de 15 documentos, no el perfil.");
Check(application.Documentos.Select(d => d.Nombre).Distinct().Count() == 15, "Sin duplicados.");
Check(application.Documentos.All(d => d.ClienteId == application.ClienteId && d.FechaVencimiento == null), "Sin vencimientos y asociados al cliente.");
Check(!application.Documentos.Any(d => d.Tipo == TipoDocumentoCredito.Referencias), "Referencias no se solicita como documento.");
try
{
    Invoke("ValidateDecision", application, EstadoSolicitudCredito.EnEstudio);
    throw new Exception("Debe exigir la confirmación explícita.");
}
catch (TargetInvocationException e) when (e.InnerException is ValidationException) { }
application.DocumentacionCompleta = true;
Invoke("ValidateDecision", application, EstadoSolicitudCredito.EnEstudio);
Check(application.Documentos.All(d => d.Estado == EstadoDocumentoCredito.Pendiente), "Permite avanzar sin exigir archivos opcionales ni falsear sus estados.");
application.Estado = EstadoSolicitudCredito.DocumentosRecibidos;
application.FechaDocumentacionCompleta = DateTime.UtcNow;
application.UsuarioDocumentacionCompleta = "tester";
Invoke("ClearDocumentationConfirmation", application);
Check(!application.DocumentacionCompleta && application.FechaDocumentacionCompleta == null && application.UsuarioDocumentacionCompleta == null, "Cambiar documentos revoca la confirmación.");
Check(application.Estado == EstadoSolicitudCredito.DocumentosPendientes, "Reabre el estado documental.");
application.Estado = EstadoSolicitudCredito.Aprobada;
Invoke("ClearDocumentationConfirmation", application);
Check(application.Estado == EstadoSolicitudCredito.Aprobada, "No altera decisiones de crédito existentes.");
Console.WriteLine("OK: catálogo, perfil ignorado, fechas, confirmación, documentos opcionales y reapertura.");

static CreditCoDebtorDto Person(string id) => new(null, "Persona " + id, id, "300" + id, "Familiar", 2000000,
    "Referencia 1", "310" + id, "Amigo", "Referencia 2", "320" + id, "Familiar");
var multi = new SolicitudCredito { EmpresaId = Guid.NewGuid(), FechaPrimerVencimiento = new DateTime(2026, 10, 20) };
Check(CreditCoDebtorService.Apply(multi, [Person("100"), Person("200")]), "Agregar dos codeudores debe registrar un cambio.");
Check(multi.Codeudores.Count == 2 && multi.Codeudores.All(x => x.Activo && x.EmpresaId == multi.EmpresaId && x.SolicitudCreditoId == multi.Id), "Codeudores vinculados a la solicitud y empresa.");
var people = multi.Codeudores.OrderBy(x => x.Orden).Select(CreditCoDebtorService.ToDto).ToArray();
Check(!CreditCoDebtorService.Apply(multi, people), "Guardar sin cambios no revoca confirmaciones.");
Check(multi.CodeudorNombre == people[0].Name, "Conservar compatibilidad con el primer codeudor.");
var file = new DocumentoSolicitudCredito { CodeudorId = people[0].Id, RutaArchivo = "existing-file.pdf", SolicitudCreditoId = multi.Id };
multi.Documentos.Add(file);
Check(CreditCoDebtorService.Apply(multi, [people[1] with { Name = "Nombre editado" }]), "Editar y retirar detecta cambios.");
Check(multi.Codeudores.Count == 2 && multi.Codeudores.Count(x => x.Activo) == 1, "Retirar no elimina el registro histórico.");
Check(multi.Codeudores.Single(x => x.Activo).Id == people[1].Id, "Editar conserva el identificador.");
Check(file.CodeudorId == people[0].Id && file.RutaArchivo == "existing-file.pdf", "No reasigna ni borra archivos al retirar.");
Check(!CreditCoDebtorService.Apply(multi, null) && multi.Codeudores.Count(x => x.Activo) == 1, "Omitir colección conserva codeudores.");
try
{
    CreditCoDebtorService.Apply(multi, [Person("300") with { Id = Guid.NewGuid() }]);
    throw new Exception("Debe rechazar identificadores de otra solicitud.");
}
catch (ValidationException) { }
Check(multi.Codeudores.Count(x => x.Activo) == 1, "Validación inválida no modifica codeudores.");
try
{
    CreditCoDebtorService.Apply(multi, [Person("300"), Person("300")]);
    throw new Exception("Debe rechazar codeudores repetidos.");
}
catch (ValidationException) { }
Check(CreditCoDebtorService.Apply(multi, []) && multi.Codeudores.All(x => !x.Activo), "Lista vacía retira a todos.");
Check(multi.CodeudorNombre == null && multi.FechaPrimerVencimiento == new DateTime(2026, 10, 20), "Retirar limpia el espejo, no el vencimiento.");
var legacy = new SolicitudCredito { CodeudorNombre = "Histórico", CodeudorIdentificacion = "123", CodeudorCelular = "300" };
CreditCoDebtorService.Apply(legacy, null);
Check(legacy.Codeudores.Single().Nombre == "Histórico", "Cliente antiguo migra su codeudor sin perder datos.");
Console.WriteLine("OK: varios codeudores, edición, retiro, identidad estable, archivos, validación y compatibilidad.");

var tenant = new TestTenant();
using var db = new CrmDbContext(new DbContextOptionsBuilder<CrmDbContext>()
    .UseSqlServer("Server=(local);Database=ModelChecks;Integrated Security=True;TrustServerCertificate=True").Options, tenant);
var tracked = new SolicitudCredito { EmpresaId = tenant.EmpresaId!.Value, ClienteId = Guid.NewGuid() };
Invoke("AddChecklistDocuments", tracked, null);
db.Attach(tracked);
var controller = new CreditApplicationsController(db, null!, tenant);
var sync = typeof(CreditApplicationsController).GetMethod("SyncCoDebtors", BindingFlags.Instance | BindingFlags.NonPublic)!;
sync.Invoke(controller, [tracked, new[] { Person("400"), Person("500") }]);
db.ChangeTracker.DetectChanges();
Check(tracked.Documentos.Count == 45, "Catálogos separados: 15 cliente y 15 por cada codeudor.");
Check(tracked.Documentos.Where(d => d.CodeudorId.HasValue).All(d => d.ClienteId == null), "Los documentos del codeudor no se atribuyen al cliente.");
Check(tracked.Codeudores.All(c => tracked.Documentos.Count(d => d.CodeudorId == c.Id) == 15), "Cada codeudor recibe su propio catálogo.");
Check(db.ChangeTracker.Entries<CodeudorSolicitudCredito>().All(e => e.State == EntityState.Added), "Nuevos GUID se insertan; no se intentan actualizar filas inexistentes.");
Check(db.ChangeTracker.Entries<DocumentoSolicitudCredito>().Count(e => e.State == EntityState.Added) == 30, "Se insertan sólo documentos nuevos.");
var active = tracked.Codeudores.Select(CreditCoDebtorService.ToDto).ToArray();
sync.Invoke(controller, [tracked, active]);
Check(tracked.Documentos.Count == 45, "Editar no duplica documentos.");
var query = db.CodeudoresSolicitudCredito.ToQueryString();
Check(query.Contains("EmpresaId"), "La consulta filtra por empresa.");
Check(db.Model.FindEntityType(typeof(CodeudorSolicitudCredito))!.GetQueryFilter() != null, "Filtro tenant explícito.");
tracked.Cliente = new Cliente { Nombre = "Cliente prueba" };
tracked.Producto = new Producto { Nombre = "Producto prueba" };
tracked.FechaPrimerVencimiento = new DateTime(2026, 10, 20);
var dto = (CreditApplicationDto)typeof(CreditApplicationsController).GetMethod("ToDto", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [tracked])!;
Check(dto.CoDebtors?.Count == 2 && dto.FirstDueDate == tracked.FechaPrimerVencimiento, "DTO conserva codeudores y vencimiento.");
var lines = (List<string>)typeof(SimplePdfGenerator).GetMethod("CreditRequest", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [dto, "Empresa prueba"])!;
Check(lines.Contains("CODEUDOR 1") && lines.Contains("CODEUDOR 2"), "PDF incluye todos los codeudores.");
Check(lines.Any(line => line.StartsWith("Primer vencimiento acordado:") && line.Contains("2026")), "PDF incluye primer vencimiento.");
Check(lines.Any(line => line.StartsWith("Codeudor Persona 400 -")) && lines.Any(line => line.StartsWith("Cliente Cliente prueba -")), "PDF identifica el dueño de los documentos.");
Console.WriteLine("OK: catálogos separados, seguimiento EF, no duplicación, tenant, DTO y contenido PDF.");

sealed class TestTenant : ITenantContext
{
    public Guid? EmpresaId { get; private set; } = Guid.NewGuid();
    public string? Subdominio => "test";
    public string UsuarioActual => "test";
    public void SetTenant(Guid empresaId, string? subdominio = null) => EmpresaId = empresaId;
}
