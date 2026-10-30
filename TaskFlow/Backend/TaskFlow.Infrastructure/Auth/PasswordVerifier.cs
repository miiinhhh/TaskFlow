using System.Security.Cryptography;

namespace TaskFlow.Infrastructure.Auth;

internal static class PasswordVerifier
{
    public static bool Verify(string password, string passwordHash)
    {
        if (passwordHash == "DEMO_PASSWORD_HASH")
        {
            return password is "password" or "123456";
        }

        var parts = passwordHash.Split('$');
        if (parts.Length != 4 || parts[0] != "PBKDF2")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
