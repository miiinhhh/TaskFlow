using System.Security.Cryptography;

namespace TaskFlow.Infrastructure.Auth;

internal static class PasswordHasher
{
    private const int Iterations = 100000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        string saltText = Convert.ToBase64String(salt);
        string hashText = Convert.ToBase64String(hash);

        return "PBKDF2$" + Iterations + "$" + saltText + "$" + hashText;
    }
}
