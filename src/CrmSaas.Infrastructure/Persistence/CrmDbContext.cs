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
    public DbSet<ProductoFoto> ProductoFotos => Set<ProductoFoto>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<SolicitudCredito> SolicitudesCredito => Set<SolicitudCredito>();
    public DbSet<DocumentoSolicitudCredito> DocumentosSolicitudCredito => Set<DocumentoSolicitudCredito>();
    public DbSet<EntregaMoto> EntregasMoto => Set<EntregaMoto>();

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
        modelBuilder.Entity<ProductoFoto>().ToTable("ProductoFotos");
        modelBuilder.Entity<Cotizacion>().ToTable("Cotizaciones");
        modelBuilder.Entity<SolicitudCredito>().ToTable("SolicitudesCredito");
        modelBuilder.Entity<DocumentoSolicitudCredito>().ToTable("DocumentosSolicitudCredito");
        modelBuilder.Entity<EntregaMoto>().ToTable("EntregasMoto");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(t => typeof(AuditableTenantEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).HasIndex(nameof(AuditableTenantEntity.EmpresaId));
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableTenantEntity.UsuarioCreacion)).HasMaxLength(180);
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableTenantEntity.UsuarioActualizacion)).HasMaxLength(180);
        }

        modelBuilder.Entity<Empresa>().HasIndex(x => x.Subdominio).IsUnique();
        modelBuilder.Entity<Empresa>().Property(x => x.LogoDataUrl).HasMaxLength(300000);
        modelBuilder.Entity<Usuario>().HasIndex(x => new { x.EmpresaId, x.Email }).IsUnique();
        modelBuilder.Entity<Rol>().HasIndex(x => new { x.EmpresaId, x.Nombre }).IsUnique();
        modelBuilder.Entity<UsuarioRol>().HasIndex(x => new { x.EmpresaId, x.UsuarioId, x.RolId }).IsUnique();
        modelBuilder.Entity<Cliente>().HasIndex(x => new { x.EmpresaId, x.Email });
        modelBuilder.Entity<Prospecto>().HasIndex(x => new { x.EmpresaId, x.Email });
        modelBuilder.Entity<Negocio>().Property(x => x.Valor).HasPrecision(18, 2);
        modelBuilder.Entity<Negocio>().Property(x => x.ProbabilidadCierre).HasPrecision(5, 2);
        modelBuilder.Entity<EtapaNegocio>().Property(x => x.ProbabilidadPredeterminada).HasPrecision(5, 2);
        modelBuilder.Entity<Producto>().Property(x => x.Nombre).HasMaxLength(180);
        modelBuilder.Entity<Producto>().Property(x => x.Categoria).HasMaxLength(80);
        modelBuilder.Entity<Producto>().Property(x => x.Precio).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().HasIndex(x => new { x.EmpresaId, x.Categoria });
        modelBuilder.Entity<Producto>().HasIndex(x => new { x.EmpresaId, x.Referencia });
        modelBuilder.Entity<ProductoFoto>().Property(x => x.NombreArchivo).HasMaxLength(220);
        modelBuilder.Entity<ProductoFoto>().Property(x => x.ContentType).HasMaxLength(80);
        modelBuilder.Entity<ProductoFoto>().HasIndex(x => new { x.EmpresaId, x.ProductoId });
        modelBuilder.Entity<ProductoFoto>().HasOne(x => x.Producto).WithMany(x => x.Fotos).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Cotizacion>().Property(x => x.PrecioProducto).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.CuotaInicial).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.Seguro).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.GastosAdministrativos).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.TasaInteresMensual).HasPrecision(6, 3);
        modelBuilder.Entity<Cotizacion>().Property(x => x.ValorFinanciado).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.CuotaMensualEstimada).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.TotalPagarEstimado).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.IngresosMensuales).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.CuotaInicial).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.ValorMoto).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.CodeudorIngresosMensuales).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.UsuarioDecision).HasMaxLength(180);
        modelBuilder.Entity<SolicitudCredito>().HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
        modelBuilder.Entity<DocumentoSolicitudCredito>().HasIndex(x => new { x.EmpresaId, x.SolicitudCreditoId, x.Tipo });
        modelBuilder.Entity<EntregaMoto>().Property(x => x.Numero).HasMaxLength(40);
        modelBuilder.Entity<EntregaMoto>().Property(x => x.AsesorResponsable).HasMaxLength(180);
        modelBuilder.Entity<EntregaMoto>().Property(x => x.Vin).HasMaxLength(80);
        modelBuilder.Entity<EntregaMoto>().Property(x => x.NumeroChasis).HasMaxLength(80);
        modelBuilder.Entity<EntregaMoto>().Property(x => x.NumeroMotor).HasMaxLength(80);
        modelBuilder.Entity<EntregaMoto>().Property(x => x.Placa).HasMaxLength(20);
        modelBuilder.Entity<EntregaMoto>().HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
        modelBuilder.Entity<EntregaMoto>().HasIndex(x => new { x.EmpresaId, x.SolicitudCreditoId }).IsUnique();
        modelBuilder.Entity<EntregaMoto>().HasOne(x => x.SolicitudCredito).WithMany().HasForeignKey(x => x.SolicitudCreditoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<EntregaMoto>().HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<EntregaMoto>().HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);

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
        modelBuilder.Entity<ProductoFoto>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Cotizacion>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<SolicitudCredito>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<DocumentoSolicitudCredito>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<EntregaMoto>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = ColombiaTime.Now;
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
