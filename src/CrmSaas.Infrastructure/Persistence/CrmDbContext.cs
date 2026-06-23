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
    public DbSet<ConfiguracionFinancieraEmpresa> ConfiguracionesFinancierasEmpresa => Set<ConfiguracionFinancieraEmpresa>();
    public DbSet<PuntoVenta> PuntosVenta => Set<PuntoVenta>();
    public DbSet<PerfilRequisito> PerfilesRequisito => Set<PerfilRequisito>();
    public DbSet<DocumentoPerfilRequisito> DocumentosPerfilRequisito => Set<DocumentoPerfilRequisito>();
    public DbSet<Promocion> Promociones => Set<Promocion>();
    public DbSet<Cotizacion> Cotizaciones => Set<Cotizacion>();
    public DbSet<CotizacionItem> CotizacionItems => Set<CotizacionItem>();
    public DbSet<SolicitudCredito> SolicitudesCredito => Set<SolicitudCredito>();
    public DbSet<DocumentoSolicitudCredito> DocumentosSolicitudCredito => Set<DocumentoSolicitudCredito>();
    public DbSet<EntregaMoto> EntregasMoto => Set<EntregaMoto>();
    public DbSet<OrdenRecaudo> OrdenesRecaudo => Set<OrdenRecaudo>();
    public DbSet<DetalleOrdenRecaudo> DetallesOrdenRecaudo => Set<DetalleOrdenRecaudo>();

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
        modelBuilder.Entity<ConfiguracionFinancieraEmpresa>().ToTable("ConfiguracionesFinancierasEmpresa");
        modelBuilder.Entity<PuntoVenta>().ToTable("PuntosVenta");
        modelBuilder.Entity<PerfilRequisito>().ToTable("PerfilesRequisito");
        modelBuilder.Entity<DocumentoPerfilRequisito>().ToTable("DocumentosPerfilRequisito");
        modelBuilder.Entity<Promocion>().ToTable("Promociones");
        modelBuilder.Entity<Cotizacion>().ToTable("Cotizaciones");
        modelBuilder.Entity<CotizacionItem>().ToTable("CotizacionItems");
        modelBuilder.Entity<SolicitudCredito>().ToTable("SolicitudesCredito");
        modelBuilder.Entity<DocumentoSolicitudCredito>().ToTable("DocumentosSolicitudCredito");
        modelBuilder.Entity<EntregaMoto>().ToTable("EntregasMoto");
        modelBuilder.Entity<OrdenRecaudo>().ToTable("OrdenesRecaudo");
        modelBuilder.Entity<DetalleOrdenRecaudo>().ToTable("DetallesOrdenRecaudo");

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(t => typeof(AuditableTenantEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).HasIndex(nameof(AuditableTenantEntity.EmpresaId));
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableTenantEntity.UsuarioCreacion)).HasMaxLength(180);
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableTenantEntity.UsuarioActualizacion)).HasMaxLength(180);
        }

        modelBuilder.Entity<Empresa>().HasIndex(x => x.Subdominio).IsUnique();
        modelBuilder.Entity<Empresa>().Property(x => x.LogoDataUrl).HasMaxLength(300000);
        modelBuilder.Entity<Usuario>().HasIndex(x => new { x.EmpresaId, x.Email }).IsUnique();
        modelBuilder.Entity<Usuario>().HasIndex(x => new { x.EmpresaId, x.PuntoVentaId });
        modelBuilder.Entity<Usuario>().HasOne(x => x.PuntoVenta).WithMany().HasForeignKey(x => x.PuntoVentaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Rol>().HasIndex(x => new { x.EmpresaId, x.Nombre }).IsUnique();
        modelBuilder.Entity<UsuarioRol>().HasIndex(x => new { x.EmpresaId, x.UsuarioId, x.RolId }).IsUnique();
        modelBuilder.Entity<Cliente>().HasIndex(x => new { x.EmpresaId, x.Email });
        modelBuilder.Entity<Prospecto>().HasIndex(x => new { x.EmpresaId, x.Email });
        modelBuilder.Entity<Negocio>().Property(x => x.Valor).HasPrecision(18, 2);
        modelBuilder.Entity<Negocio>().Property(x => x.ProbabilidadCierre).HasPrecision(5, 2);
        modelBuilder.Entity<EtapaNegocio>().Property(x => x.ProbabilidadPredeterminada).HasPrecision(5, 2);
        modelBuilder.Entity<Producto>().Property(x => x.Nombre).HasMaxLength(180);
        modelBuilder.Entity<Producto>().Property(x => x.Categoria).HasMaxLength(80);
        modelBuilder.Entity<Producto>().Property(x => x.Linea).HasMaxLength(100);
        modelBuilder.Entity<Producto>().Property(x => x.Version).HasMaxLength(100);
        modelBuilder.Entity<Producto>().Property(x => x.Precio).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().Property(x => x.Soat).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().Property(x => x.Matricula).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().Property(x => x.Impuestos).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().Property(x => x.FichaTecnica).HasMaxLength(1600);
        modelBuilder.Entity<Producto>().HasIndex(x => new { x.EmpresaId, x.Categoria });
        modelBuilder.Entity<Producto>().HasIndex(x => new { x.EmpresaId, x.Referencia });
        modelBuilder.Entity<ProductoFoto>().Property(x => x.NombreArchivo).HasMaxLength(220);
        modelBuilder.Entity<ProductoFoto>().Property(x => x.ContentType).HasMaxLength(80);
        modelBuilder.Entity<ProductoFoto>().HasIndex(x => new { x.EmpresaId, x.ProductoId });
        modelBuilder.Entity<ProductoFoto>().HasOne(x => x.Producto).WithMany(x => x.Fotos).HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ConfiguracionFinancieraEmpresa>().Property(x => x.SalarioMinimoVigente).HasPrecision(18, 2);
        modelBuilder.Entity<ConfiguracionFinancieraEmpresa>().Property(x => x.TasaConsumoEa).HasPrecision(6, 3);
        modelBuilder.Entity<ConfiguracionFinancieraEmpresa>().Property(x => x.TasaBajoMontoEa).HasPrecision(6, 3);
        modelBuilder.Entity<ConfiguracionFinancieraEmpresa>().Property(x => x.TasaFactorMensual).HasPrecision(6, 3);
        modelBuilder.Entity<ConfiguracionFinancieraEmpresa>().HasIndex(x => x.EmpresaId).IsUnique();
        modelBuilder.Entity<PuntoVenta>().Property(x => x.Nombre).HasMaxLength(160);
        modelBuilder.Entity<PuntoVenta>().Property(x => x.Codigo).HasMaxLength(40);
        modelBuilder.Entity<PuntoVenta>().Property(x => x.Ciudad).HasMaxLength(120);
        modelBuilder.Entity<PuntoVenta>().Property(x => x.MarcaPrincipal).HasMaxLength(80);
        modelBuilder.Entity<PuntoVenta>().Property(x => x.ModalidadEntrega).HasMaxLength(40);
        modelBuilder.Entity<PuntoVenta>().Property(x => x.LogoMarcaDataUrl).HasMaxLength(300000);
        modelBuilder.Entity<PuntoVenta>().Property(x => x.TasaFactorMensual).HasPrecision(6, 3);
        modelBuilder.Entity<PuntoVenta>().Property(x => x.CondicionesComerciales).HasMaxLength(1200);
        modelBuilder.Entity<PuntoVenta>().HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
        modelBuilder.Entity<PuntoVenta>().HasIndex(x => new { x.EmpresaId, x.Ciudad });
        modelBuilder.Entity<PerfilRequisito>().Property(x => x.Nombre).HasMaxLength(120);
        modelBuilder.Entity<PerfilRequisito>().Property(x => x.Codigo).HasMaxLength(40);
        modelBuilder.Entity<PerfilRequisito>().Property(x => x.Descripcion).HasMaxLength(500);
        modelBuilder.Entity<PerfilRequisito>().HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
        modelBuilder.Entity<DocumentoPerfilRequisito>().Property(x => x.Nombre).HasMaxLength(160);
        modelBuilder.Entity<DocumentoPerfilRequisito>().Property(x => x.Descripcion).HasMaxLength(500);
        modelBuilder.Entity<DocumentoPerfilRequisito>().HasIndex(x => new { x.EmpresaId, x.PerfilRequisitoId, x.Orden });
        modelBuilder.Entity<DocumentoPerfilRequisito>().HasOne(x => x.PerfilRequisito).WithMany(x => x.Documentos).HasForeignKey(x => x.PerfilRequisitoId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Promocion>().Property(x => x.Nombre).HasMaxLength(160);
        modelBuilder.Entity<Promocion>().Property(x => x.Codigo).HasMaxLength(50);
        modelBuilder.Entity<Promocion>().Property(x => x.TipoDescuento).HasMaxLength(20);
        modelBuilder.Entity<Promocion>().Property(x => x.ValorDescuento).HasPrecision(18, 2);
        modelBuilder.Entity<Promocion>().Property(x => x.Marca).HasMaxLength(100);
        modelBuilder.Entity<Promocion>().Property(x => x.Color).HasMaxLength(80);
        modelBuilder.Entity<Promocion>().HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique();
        modelBuilder.Entity<Promocion>().HasIndex(x => new { x.EmpresaId, x.Activa, x.VigenteDesde, x.VigenteHasta });
        modelBuilder.Entity<Promocion>().HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Promocion>().HasOne(x => x.PuntoVenta).WithMany().HasForeignKey(x => x.PuntoVentaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Cotizacion>().Property(x => x.PrecioProducto).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.DescuentoPromocion).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.NombrePromocion).HasMaxLength(160);
        modelBuilder.Entity<Cotizacion>().Property(x => x.CuotaInicial).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.Seguro).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.GastosAdministrativos).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.TasaInteresMensual).HasPrecision(6, 3);
        modelBuilder.Entity<Cotizacion>().Property(x => x.TasaFactorMensualSede).HasPrecision(6, 3);
        modelBuilder.Entity<Cotizacion>().Property(x => x.NombreSede).HasMaxLength(160);
        modelBuilder.Entity<Cotizacion>().Property(x => x.MarcaSede).HasMaxLength(80);
        modelBuilder.Entity<Cotizacion>().Property(x => x.ModalidadEntregaSede).HasMaxLength(40);
        modelBuilder.Entity<Cotizacion>().Property(x => x.CondicionesSede).HasMaxLength(1200);
        modelBuilder.Entity<Cotizacion>().Property(x => x.ValorFinanciado).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.CuotaMensualEstimada).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.TotalPagarEstimado).HasPrecision(18, 2);
        modelBuilder.Entity<Cotizacion>().Property(x => x.TipoCredito).HasMaxLength(40);
        modelBuilder.Entity<Cotizacion>().HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
        modelBuilder.Entity<Cotizacion>().HasIndex(x => new { x.EmpresaId, x.PuntoVentaId });
        modelBuilder.Entity<Cotizacion>().HasIndex(x => new { x.EmpresaId, x.PerfilRequisitoId });
        modelBuilder.Entity<Cotizacion>().HasIndex(x => new { x.EmpresaId, x.PromocionId });
        modelBuilder.Entity<Cotizacion>().HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Cotizacion>().HasOne(x => x.PuntoVenta).WithMany().HasForeignKey(x => x.PuntoVentaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Cotizacion>().HasOne(x => x.PerfilRequisito).WithMany().HasForeignKey(x => x.PerfilRequisitoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Cotizacion>().HasOne(x => x.Promocion).WithMany().HasForeignKey(x => x.PromocionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.PrecioProducto).HasPrecision(18, 2);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.DescuentoPromocion).HasPrecision(18, 2);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.NombrePromocion).HasMaxLength(160);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.CuotaInicial).HasPrecision(18, 2);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.Seguro).HasPrecision(18, 2);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.GastosAdministrativos).HasPrecision(18, 2);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.TasaInteresMensual).HasPrecision(6, 3);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.ValorFinanciado).HasPrecision(18, 2);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.CuotaMensualEstimada).HasPrecision(18, 2);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.TotalPagarEstimado).HasPrecision(18, 2);
        modelBuilder.Entity<CotizacionItem>().Property(x => x.TipoCredito).HasMaxLength(40);
        modelBuilder.Entity<CotizacionItem>().HasIndex(x => new { x.EmpresaId, x.CotizacionId });
        modelBuilder.Entity<CotizacionItem>().HasOne(x => x.Cotizacion).WithMany(x => x.Items).HasForeignKey(x => x.CotizacionId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CotizacionItem>().HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CotizacionItem>().HasOne(x => x.Promocion).WithMany().HasForeignKey(x => x.PromocionId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.IngresosMensuales).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.CuotaInicial).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.ValorMoto).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.CodeudorIngresosMensuales).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.UsuarioPaso0).HasMaxLength(180);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.ResultadoEstudio).HasMaxLength(60);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.ValorAprobadoAnalista).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.CuotaInicialAprobada).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.CuotaMensualAprobada).HasPrecision(18, 2);
        modelBuilder.Entity<SolicitudCredito>().Property(x => x.UsuarioDecision).HasMaxLength(180);
        modelBuilder.Entity<SolicitudCredito>().HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
        modelBuilder.Entity<SolicitudCredito>().HasIndex(x => new { x.EmpresaId, x.PerfilRequisitoId });
        modelBuilder.Entity<SolicitudCredito>().HasOne(x => x.PerfilRequisito).WithMany().HasForeignKey(x => x.PerfilRequisitoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DocumentoSolicitudCredito>().Property(x => x.MotivoRechazo).HasMaxLength(500);
        modelBuilder.Entity<DocumentoSolicitudCredito>().Property(x => x.UsuarioValidacion).HasMaxLength(180);
        modelBuilder.Entity<DocumentoSolicitudCredito>().HasIndex(x => new { x.EmpresaId, x.SolicitudCreditoId, x.Tipo });
        modelBuilder.Entity<DocumentoSolicitudCredito>().HasIndex(x => new { x.EmpresaId, x.ClienteId, x.Tipo });
        modelBuilder.Entity<DocumentoSolicitudCredito>().HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
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
        modelBuilder.Entity<OrdenRecaudo>().Property(x => x.Numero).HasMaxLength(40);
        modelBuilder.Entity<OrdenRecaudo>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<OrdenRecaudo>().Property(x => x.ValorPagado).HasPrecision(18, 2);
        modelBuilder.Entity<OrdenRecaudo>().HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique();
        modelBuilder.Entity<OrdenRecaudo>().HasIndex(x => new { x.EmpresaId, x.SolicitudCreditoId });
        modelBuilder.Entity<OrdenRecaudo>().HasIndex(x => new { x.EmpresaId, x.Estado, x.FechaVencimiento });
        modelBuilder.Entity<OrdenRecaudo>().HasOne(x => x.SolicitudCredito).WithMany().HasForeignKey(x => x.SolicitudCreditoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<OrdenRecaudo>().HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DetalleOrdenRecaudo>().Property(x => x.Concepto).HasMaxLength(120);
        modelBuilder.Entity<DetalleOrdenRecaudo>().Property(x => x.Valor).HasPrecision(18, 2);
        modelBuilder.Entity<DetalleOrdenRecaudo>().HasIndex(x => new { x.EmpresaId, x.OrdenRecaudoId, x.Tipo });
        modelBuilder.Entity<DetalleOrdenRecaudo>().HasOne(x => x.OrdenRecaudo).WithMany(x => x.Detalles).HasForeignKey(x => x.OrdenRecaudoId).OnDelete(DeleteBehavior.Cascade);

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
        modelBuilder.Entity<ConfiguracionFinancieraEmpresa>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<PuntoVenta>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<PerfilRequisito>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<DocumentoPerfilRequisito>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Promocion>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<Cotizacion>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<CotizacionItem>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<SolicitudCredito>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<DocumentoSolicitudCredito>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<EntregaMoto>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<OrdenRecaudo>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
        modelBuilder.Entity<DetalleOrdenRecaudo>().HasQueryFilter(x => tenantContext.EmpresaId.HasValue && x.EmpresaId == tenantContext.EmpresaId.Value);
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
