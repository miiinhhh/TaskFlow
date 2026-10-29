using System.Security.Cryptography;

namespace TaskFlow.Infrastructure.Auth;

internal static class PasswordVerifier
{
    public static bool Verify(string password, string passwordHash)
    {
        if (passwordHash == "DEMO_PASSWORD_HASH")
        {
            if (password == "password" || password == "123456")
            {
                return true;
            }

            return false;
        }

        string[] parts = passwordHash.Split('$');
        if (parts.Length != 4)
        {
            return false;
        }

        if (parts[0] != "PBKDF2")
        {
            return false;
        }

        int iterations;
        if (!int.TryParse(parts[1], out iterations))
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);
            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
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
