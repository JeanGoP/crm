using System.Reflection;
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
