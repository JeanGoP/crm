using CrmSaas.Domain.Common;
using CrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrmSaas.Infrastructure.Auth;

public static class UserPasswordPolicy
{
    public const string SharedPassword = "Crm2024*";

    public static bool IsSuperUser(string email) =>
        string.Equals(email.Trim(), "admin@demo.com", StringComparison.OrdinalIgnoreCase);

    public static string HashForUpdate(string existingEmail, string existingHash, IPasswordHasher hasher) =>
        IsSuperUser(existingEmail) ? existingHash : hasher.Hash(SharedPassword);

    // Apply to existing accounts across all companies before accepting requests.
    // Verify first so restarts preserve already-correct hashes and audit dates.
    public static async Task ApplyToExistingUsersAsync(CrmDbContext db, IPasswordHasher hasher)
    {
        var users = await db.Usuarios.IgnoreQueryFilters().AsNoTracking()
            .Select(x => new { x.Id, x.Email, x.PasswordHash }).ToListAsync();
        foreach (var user in users)
        {
            if (IsSuperUser(user.Email) || hasher.Verify(SharedPassword, user.PasswordHash)) continue;
            var hash = hasher.Hash(SharedPassword);
            var now = ColombiaTime.Now;
            await db.Usuarios.IgnoreQueryFilters()
                .Where(x => x.Id == user.Id && x.Email == user.Email && x.PasswordHash == user.PasswordHash)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.PasswordHash, hash)
                    .SetProperty(x => x.FechaActualizacion, now)
                    .SetProperty(x => x.UsuarioActualizacion, "shared-password-policy"));
        }
    }
}
