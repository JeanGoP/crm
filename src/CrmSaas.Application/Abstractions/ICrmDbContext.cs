using CrmSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Application.Abstractions;

public interface ICrmDbContext
{
    DbSet<Empresa> Empresas { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Rol> Roles { get; }
    DbSet<UsuarioRol> UsuarioRoles { get; }
    DbSet<UsuarioSedeSupervisada> UsuariosSedesSupervisadas { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Cliente> Clientes { get; }
    DbSet<Prospecto> Prospectos { get; }
    DbSet<Negocio> Negocios { get; }
    DbSet<EtapaNegocio> EtapasNegocio { get; }
    DbSet<Actividad> Actividades { get; }
    DbSet<Nota> Notas { get; }
    DbSet<Archivo> Archivos { get; }
    DbSet<CategoriaProducto> CategoriasProducto { get; }
    DbSet<Producto> Productos { get; }
    DbSet<ProductoFoto> ProductoFotos { get; }
    DbSet<ProductoPrecioSede> ProductoPreciosSede { get; }
    DbSet<InventarioComercial> InventarioComercial { get; }
    DbSet<ConfiguracionFinancieraEmpresa> ConfiguracionesFinancierasEmpresa { get; }
    DbSet<PuntoVenta> PuntosVenta { get; }
    DbSet<PerfilRequisito> PerfilesRequisito { get; }
    DbSet<DocumentoPerfilRequisito> DocumentosPerfilRequisito { get; }
    DbSet<Promocion> Promociones { get; }
    DbSet<Cotizacion> Cotizaciones { get; }
    DbSet<CotizacionItem> CotizacionItems { get; }
    DbSet<SolicitudCredito> SolicitudesCredito { get; }
    DbSet<DocumentoSolicitudCredito> DocumentosSolicitudCredito { get; }
    DbSet<EntregaMoto> EntregasMoto { get; }
    DbSet<OrdenRecaudo> OrdenesRecaudo { get; }
    DbSet<DetalleOrdenRecaudo> DetallesOrdenRecaudo { get; }
    DbSet<Tramite> Tramites { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
