using CrmSaas.Domain.Entities;
using CrmSaas.Domain.Enums;

namespace CrmSaas.Api.Services;

public static class CreditDocumentCatalog
{
    public static readonly string[] Names =
    [
        "Cédula de ciudadanía", "Recibo de servicio público", "Carta laboral",
        "Desprendibles de pago", "Certificado de libertad y tradición",
        "Compraventa o escritura", "Tarjeta de propiedad", "Sana posesión",
        "Extractos bancarios", "Declaración de renta", "RUT",
        "Certificado de vacunación de ganado", "Cámara de comercio", "Registro de hierro", "Otros"
    ];

    public static IEnumerable<DocumentoSolicitudCredito> Create() => Names.Select((name, index) => new DocumentoSolicitudCredito
    {
        Nombre = name,
        Tipo = index == 0 ? TipoDocumentoCredito.Cedula : index == 1 ? TipoDocumentoCredito.ReciboServicio : TipoDocumentoCredito.Otro
    });
}
