using CrmSaas.Infrastructure.Auth;

var hasher = new PasswordHasher();
var previousHash = hasher.Hash("Previous-test-password");
void Check(bool result, string message)
{
    if (!result) throw new InvalidOperationException(message);
}

Check(UserPasswordPolicy.IsSuperUser(" ADMIN@DEMO.COM "), "Superuser comparison must ignore case and surrounding spaces.");
Check(!UserPasswordPolicy.IsSuperUser("other@demo.com"), "Other accounts must not be exempt.");
Check(UserPasswordPolicy.HashForUpdate("admin@demo.com", previousHash, hasher) == previousHash,
    "Editing the superuser must preserve the exact existing password hash.");
var updatedHash = UserPasswordPolicy.HashForUpdate("seller@example.test", previousHash, hasher);
Check(hasher.Verify("Crm2024*", updatedHash), "Normal accounts must accept the requested shared password.");
Check(!hasher.Verify("Previous-test-password", updatedHash), "The previous password must no longer work.");
var nextHash = UserPasswordPolicy.HashForUpdate("seller@example.test", updatedHash, hasher);
Check(hasher.Verify("Crm2024*", nextHash), "Repeated edits must retain the shared password.");
Check(nextHash != updatedHash, "Password hashes must use independent salts.");
Console.WriteLine("Password policy: 7 checks passed.");
