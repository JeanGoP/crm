namespace CrmSaas.Infrastructure.Auth;

public static class UserPasswordPolicy
{
    public const string SharedPassword = "Crm2024*";

    public static bool IsSuperUser(string email) =>
        string.Equals(email.Trim(), "admin@demo.com", StringComparison.OrdinalIgnoreCase);

    public static string HashForUpdate(string existingEmail, string existingHash, IPasswordHasher hasher) =>
        IsSuperUser(existingEmail) ? existingHash : hasher.Hash(SharedPassword);

}
