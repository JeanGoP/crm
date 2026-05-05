using CrmSaas.Application.Abstractions;
using CrmSaas.Domain.Common;
using CrmSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Infrastructure.Persistence;

public sealed class CrmDbContext(DbContextOptions<CrmDbContext> options, ITenantContext tenantContext) : DbContext(options), ICrmDbContext
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Prospecto> Prospectos => Set<Prospecto>();
    public DbSet<Negocio> Negocios => Set<Negocio>();
    public DbSet<EtapaNegocio> EtapasNegocio => Set<EtapaNegocio>();
    public DbSet<Actividad> Actividades => Set<Actividad>();
    public DbSet<Nota> Notas => Set<Nota>();
    public DbSet<Archivo> Archivos => Set<Archivo>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<SolicitudCredito> SolicitudesCredito => Set<SolicitudCredito>();
    public DbSet<DocumentoSolicitudCredito> DocumentosSolicitudCredito => Set<DocumentoSolicitudCredito>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>().ToTable("Empresas");
        modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        modelBuilder.Entity<Rol>().ToTable("Roles");
        modelBuilder.Entity<UsuarioRol>().ToTable("UsuariosRoles");
        modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
        modelBuilder.Entity<Cliente>().ToTable("Clientes");
        modelBuilder.Entity<Prospecto>().ToTable("Prospectos");
        modelBuilder.Entity<Negocio>().ToTable("Negocios");
        modelBuilder.Entity<EtapaNegocio>().ToTable("EtapasNegocio");
        modelBuilder.Entity<Actividad>().ToTable("Actividades");
        modelBuilder.Entity<Nota>().ToTable("Notas");
        modelBuilder.Entity<Archivo>().ToTable("Archivos");
        modelBuilder.Entity<Producto>().ToTable("Productos");
        modelBuilder.Entity<Cotizacion>().ToTable("Cotizaciones");
        modelBuilder.Entity<SolicitudCredito>().ToTable("SolicitudesCredito");
        modelBuilder.Entity<DocumentoSolicitudCredito>().ToTable("DocumentosSolicitudCredito");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(t => typeof(AuditableTenantEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).HasIndex(nameof(AuditableTenantEntity.EmpresaId));
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableTenantEntity.UsuarioCreacion)).HasMaxLength(180);
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableTenantEntity.UsuarioActualizacion)).HasMaxLength(180);
        }

        modelBuilder.Entity<Empresa>().HasIndex(x => x.Subdominio).IsUnique();
        modelBuilder.Entity<Usuario>().HasIndex(x => new { x.EmpresaId, x.Email }).IsUnique();
        modelBuilder.Entity<Rol>().HasIndex(x => new { x.EmpresaId, x.Nombre }).IsUnique();
        modelBuilder.Entity<UsuarioRol>().HasIndex(x => new { x.EmpresaId, x.UsuarioId, x.RolId }).IsUnique();
        modelBuilder.Entity<Cliente>().HasIndex(x => new { x.EmpresaId, x.Email });
        modelBuilder.Entity<Prospecto>().HasIndex(x => new { x.EmpresaId, x.Email });
        modelBuilder.Entity<Negocio>().Property(x => x.Valor).HasPrecision(18, 2);
        modelBuilder.Entity<Negocio>().Property(x => x.ProbabilidadCierre).HasPrecision(5, 2);
        modelBuilder.Entity<EtapaNegocio>().Property(x => x.ProbabilidadPredeterminada).HasPrecision(5, 2);
        modelBuilder.Entity<Producto>().Property(x => x.Precio).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().HasIndex(x => new { x.EmpresaId, x.Referencia });
        modelBuilder.Entity<Cotizacion>().Property(x => x.PrecioProducto).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.IngresosMensuales).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.CuotaInicial).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.ValorMoto).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
        modelBuilder.Entity<DocumentoSolicitudCredito>().HasIndex(x => new { x.EmpresaId, x.SolicitudCreditoId, x.Tipo });

        modelBuilder.Entity<Empresa>().HasQueryFilter(x => !tenantContext.EmpresaId.HasValue || x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Usuario>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Rol>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<UsuarioRol>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<RefreshToken>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Cliente>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Prospecto>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Negocio>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<EtapaNegocio>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Actividad>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Nota>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Archivo>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Producto>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Cotizacion>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<SolicitudCredito>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<DocumentoSolicitudCredito>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableTenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.FechaCreacion = now;
                entry.Entity.UsuarioCreacion = tenantContext.UsuarioActual;
                entry.Entity.EmpresaId = entry.Entity.EmpresaId == Guid.Empty
                    ? tenantContext.EmpresaId ?? throw new InvalidOperationException("No se resolvio el tenant para crear datos.")
                    : entry.Entity.EmpresaId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.FechaActualizacion = now;
                entry.Entity.UsuarioActualizacion = tenantContext.UsuarioActual;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
