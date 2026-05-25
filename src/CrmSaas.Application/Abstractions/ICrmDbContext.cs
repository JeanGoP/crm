using CrmSaas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Application.Abstractions;

public interface ICrmDbContext
{
    DbSet<Empresa> Empresas { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Rol> Roles { get; }
    DbSet<UsuarioRol> UsuarioRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Cliente> Clientes { get; }
    DbSet<Prospecto> Prospectos { get; }
    DbSet<Negocio> Negocios { get; }
    DbSet<EtapaNegocio> EtapasNegocio { get; }
    DbSet<Actividad> Actividades { get; }
    DbSet<Nota> Notas { get; }
    DbSet<Archivo> Archivos { get; }
    DbSet<Producto> Productos { get; }
    DbSet<Cotizacion> Cotizaciones { get; }
    DbSet<SolicitudCredito> SolicitudesCredito { get; }
    DbSet<DocumentoSolicitudCredito> DocumentosSolicitudCredito { get; }
    DbSet<EntregaMoto> EntregasMoto { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
