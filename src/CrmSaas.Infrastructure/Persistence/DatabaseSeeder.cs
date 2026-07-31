using CrmSaas.Domain.Entities;
using CrmSaas.Domain.Enums;
using CrmSaas.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    private static readonly string[] RoleNames = ["Administrador", "Vendedor", "Supervisor"];

    public static async Task SeedDemoAsync(CrmDbContext db, IPasswordHasher passwordHasher, string adminPassword, CancellationToken cancellationToken = default)
    {
        const string tenant = "demo";
        var empresa = await db.Empresas.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Subdominio == tenant, cancellationToken);
        if (empresa is null)
        {
            var empresaId = Guid.NewGuid();
            empresa = new Empresa
            {
                Id = empresaId,
                EmpresaId = empresaId,
                Nombre = "Empresa Demo",
                Subdominio = tenant,
                Activa = true
            };
            db.Empresas.Add(empresa);
        }

        await SeedCompanyDefaultsAsync(db, empresa.Id, cancellationToken);

        var admin = await db.Usuarios.IgnoreQueryFilters()
            .Include(x => x.UsuarioRoles)
            .FirstOrDefaultAsync(x => x.EmpresaId == empresa.Id && x.Email == "admin@demo.com", cancellationToken);
        var adminRole = await db.Roles.IgnoreQueryFilters()
            .FirstAsync(x => x.EmpresaId == empresa.Id && x.Nombre == "Administrador", cancellationToken);
        var defaultSalesPointId = await db.PuntosVenta.IgnoreQueryFilters()
            .Where(x => x.EmpresaId == empresa.Id && x.Activa)
            .OrderByDescending(x => x.Codigo == "PRINCIPAL")
            .ThenBy(x => x.Nombre)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (admin is null)
        {
            admin = new Usuario
            {
                EmpresaId = empresa.Id,
                NombreCompleto = "Administrador Demo",
                Login = "admin@demo.com",
                Email = "admin@demo.com",
                PasswordHash = passwordHasher.Hash(adminPassword),
                PuntoVentaId = defaultSalesPointId,
                Activo = true
            };
            admin.UsuarioRoles.Add(new UsuarioRol { EmpresaId = empresa.Id, RolId = adminRole.Id });
            db.Usuarios.Add(admin);
        }
        else if (!admin.UsuarioRoles.Any(x => x.RolId == adminRole.Id))
        {
            admin.UsuarioRoles.Add(new UsuarioRol { EmpresaId = empresa.Id, UsuarioId = admin.Id, RolId = adminRole.Id });
        }

        if (string.IsNullOrWhiteSpace(admin.Login))
        {
            admin.Login = admin.Email.ToLowerInvariant();
        }

        if (admin.PuntoVentaId is null)
        {
            admin.PuntoVentaId = defaultSalesPointId;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedCompanyDefaultsAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken = default)
    {
        var existingRoles = await db.Roles.IgnoreQueryFilters()
            .Where(x => x.EmpresaId == empresaId)
            .ToListAsync(cancellationToken);

        foreach (var roleName in RoleNames.Except(existingRoles.Select(x => x.Nombre)))
        {
            db.Roles.Add(new Rol
            {
                EmpresaId = empresaId,
                Nombre = roleName,
                Descripcion = $"Rol {roleName}"
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        if (!await db.ConfiguracionesFinancierasEmpresa.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == empresaId, cancellationToken))
        {
            db.ConfiguracionesFinancierasEmpresa.Add(new ConfiguracionFinancieraEmpresa
            {
                EmpresaId = empresaId,
                SalarioMinimoVigente = 1400000,
                TasaConsumoEa = 29.72m,
                TasaBajoMontoEa = 56.33m,
                TasaFactorMensual = 4.5m,
                PlazoMaximoMeses = 30,
                RedondeoCuota = 1000,
                UsarTablaMontelibano = true,
                Activa = true
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        if (!await db.PuntosVenta.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == empresaId, cancellationToken))
        {
            db.PuntosVenta.Add(new PuntoVenta
            {
                EmpresaId = empresaId,
                Nombre = "Sede principal",
                Codigo = "PRINCIPAL",
                Ciudad = "Montelibano",
                MarcaPrincipal = "Honda",
                TasaFactorMensual = 4.5m,
                PlazoMaximoMeses = 30,
                VigenciaCotizacionDias = 7,
                ModalidadEntrega = "ConSoat",
                TiempoSoatDias = 14,
                TiempoMatriculaDias = 20,
                CondicionesComerciales = "Cotizacion sujeta a disponibilidad del producto, validacion comercial y aprobacion final.",
                Activa = true
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        await SeedRequirementProfilesAsync(db, empresaId, cancellationToken);

        if (!await db.EtapasNegocio.IgnoreQueryFilters().AnyAsync(x => x.EmpresaId == empresaId, cancellationToken))
        {
            db.EtapasNegocio.AddRange(
                new EtapaNegocio { EmpresaId = empresaId, Nombre = "Cotizado", Orden = 1, ProbabilidadPredeterminada = 10 },
                new EtapaNegocio { EmpresaId = empresaId, Nombre = "Interesado", Orden = 2, ProbabilidadPredeterminada = 20 },
                new EtapaNegocio { EmpresaId = empresaId, Nombre = "Documentos pendientes", Orden = 3, ProbabilidadPredeterminada = 35 },
                new EtapaNegocio { EmpresaId = empresaId, Nombre = "Credito en estudio", Orden = 4, ProbabilidadPredeterminada = 65 },
                new EtapaNegocio { EmpresaId = empresaId, Nombre = "Aprobado", Orden = 5, ProbabilidadPredeterminada = 80 },
                new EtapaNegocio { EmpresaId = empresaId, Nombre = "Rechazado", Orden = 6, ProbabilidadPredeterminada = 0 },
                new EtapaNegocio { EmpresaId = empresaId, Nombre = "Entregado", Orden = 7, ProbabilidadPredeterminada = 100 },
                new EtapaNegocio { EmpresaId = empresaId, Nombre = "Desistido", Orden = 8, ProbabilidadPredeterminada = 0 });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRequirementProfilesAsync(CrmDbContext db, Guid empresaId, CancellationToken cancellationToken)
    {
        var existingCodes = await db.PerfilesRequisito.IgnoreQueryFilters()
            .Where(x => x.EmpresaId == empresaId)
            .Select(x => x.Codigo)
            .ToListAsync(cancellationToken);

        var existing = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var profiles = new[]
        {
            CreateProfile(empresaId, "Empleado", "EMPLEADO", false, "Cliente asalariado con soporte laboral e ingresos.", [
                Doc(TipoDocumentoCredito.Cedula, "Fotocopia de cedula", 1),
                Doc(TipoDocumentoCredito.SoporteIngresos, "Carta laboral o dos ultimas colillas de pago", 2),
                Doc(TipoDocumentoCredito.ReciboServicio, "Recibo de servicio publico", 3),
                Doc(TipoDocumentoCredito.Referencias, "Referencias personales", 4)
            ]),
            CreateProfile(empresaId, "Independiente", "INDEPENDIENTE", false, "Cliente independiente o comerciante.", [
                Doc(TipoDocumentoCredito.Cedula, "Fotocopia de cedula", 1),
                Doc(TipoDocumentoCredito.SoporteIngresos, "Certificado de ingresos o camara de comercio", 2),
                Doc(TipoDocumentoCredito.SoporteIngresos, "Extractos bancarios", 3),
                Doc(TipoDocumentoCredito.ReciboServicio, "Recibo de servicio publico", 4),
                Doc(TipoDocumentoCredito.Referencias, "Referencias comerciales o personales", 5)
            ]),
            CreateProfile(empresaId, "Pensionado", "PENSIONADO", false, "Cliente pensionado.", [
                Doc(TipoDocumentoCredito.Cedula, "Fotocopia de cedula", 1),
                Doc(TipoDocumentoCredito.SoporteIngresos, "Dos ultimas colillas de pension", 2),
                Doc(TipoDocumentoCredito.ReciboServicio, "Recibo de servicio publico", 3),
                Doc(TipoDocumentoCredito.Referencias, "Referencias personales", 4)
            ]),
            CreateProfile(empresaId, "Contado", "CONTADO", true, "Compra de contado con documentos minimos.", [
                Doc(TipoDocumentoCredito.Cedula, "Fotocopia de cedula", 1),
                Doc(TipoDocumentoCredito.Otro, "Soporte de pago", 2)
            ])
        };

        foreach (var profile in profiles.Where(x => !existing.Contains(x.Codigo)))
        {
            db.PerfilesRequisito.Add(profile);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static PerfilRequisito CreateProfile(Guid empresaId, string name, string code, bool isCash, string description, IReadOnlyCollection<DocumentoPerfilRequisito> documents)
    {
        var profile = new PerfilRequisito
        {
            EmpresaId = empresaId,
            Nombre = name,
            Codigo = code,
            Descripcion = description,
            EsContado = isCash,
            Activo = true
        };
        foreach (var document in documents)
        {
            document.EmpresaId = empresaId;
            profile.Documentos.Add(document);
        }
        return profile;
    }

    private static DocumentoPerfilRequisito Doc(TipoDocumentoCredito type, string name, int order) => new()
    {
        Tipo = type,
        Nombre = name,
        Obligatorio = true,
        Orden = order
    };
}
