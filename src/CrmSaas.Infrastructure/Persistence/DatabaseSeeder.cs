using CrmSaas.Domain.Entities;
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

        if (admin is null)
        {
            admin = new Usuario
            {
                EmpresaId = empresa.Id,
                NombreCompleto = "Administrador Demo",
                Email = "admin@demo.com",
                PasswordHash = passwordHasher.Hash(adminPassword),
                Activo = true
            };
            admin.UsuarioRoles.Add(new UsuarioRol { EmpresaId = empresa.Id, RolId = adminRole.Id });
            db.Usuarios.Add(admin);
        }
        else if (!admin.UsuarioRoles.Any(x => x.RolId == adminRole.Id))
        {
            admin.UsuarioRoles.Add(new UsuarioRol { EmpresaId = empresa.Id, UsuarioId = admin.Id, RolId = adminRole.Id });
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
}
