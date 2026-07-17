using CrmSaas.Domain.Common;

namespace CrmSaas.Domain.Entities;

public sealed class Usuario : AuditableTenantEntity
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid? PuntoVentaId { get; set; }
    public PuntoVenta? PuntoVenta { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public ICollection<UsuarioSedeSupervisada> SedesSupervisadas { get; set; } = new List<UsuarioSedeSupervisada>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public sealed class Rol : AuditableTenantEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}

public sealed class UsuarioRol : AuditableTenantEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid RolId { get; set; }
    public Rol? Rol { get; set; }
}

public sealed class UsuarioSedeSupervisada : AuditableTenantEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid PuntoVentaId { get; set; }
    public PuntoVenta? PuntoVenta { get; set; }
}

public sealed class RefreshToken : AuditableTenantEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public DateTime? RevocadoEn { get; set; }
    public bool Activo => RevocadoEn is null && ExpiraEn > DateTime.UtcNow;
}
