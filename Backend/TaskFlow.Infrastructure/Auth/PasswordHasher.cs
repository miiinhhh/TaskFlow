using System.Security.Cryptography;

namespace TaskFlow.Infrastructure.Auth;

internal static class PasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}

public static class PasswordHasherExtensions
{
    public static bool Verify(this string hashedPassword, string password)
    {
        var parts = hashedPassword.Split('$');
        if (parts.Length != 4 || parts[0] != "PBKDF2")
            throw new FormatException("Invalid hashed password format.");

        var iterations = int.Parse(parts[1]);
        var salt = Convert.FromBase64String(parts[2]);
        var hash = Convert.FromBase64String(parts[3]);

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            hash.Length);

        return CryptographicOperations.FixedTimeEquals(computedHash, hash);
    }
}
private static async Task<int> GetMemberRoleIdAsync(CancellationToken cancellationToken)
{
    var memberRole = await dbContext.Roles
        .AsNoTracking()
        .SingleOrDefaultAsync(r => r.Name == "Member", cancellationToken);

    if (memberRole is null)
    {
        throw new InvalidOperationException("Member role not found in the database.");
    }

    return memberRole.Id;
}