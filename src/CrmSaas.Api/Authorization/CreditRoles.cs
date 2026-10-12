using System.Security.Claims;

namespace CrmSaas.Api.Authorization;

public static class CreditRoles
{
    public const string Approver = "Aprobador de créditos";
    public const string Managers = "Administrador,Supervisor,Aprobador de créditos";

    public static bool CanManage(ClaimsPrincipal user) =>
        user.IsInRole("Administrador") || user.IsInRole("Supervisor") || user.IsInRole(Approver);
}
