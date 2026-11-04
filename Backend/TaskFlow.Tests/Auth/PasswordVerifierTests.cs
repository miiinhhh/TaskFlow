using TaskFlow.Infrastructure.Auth;

namespace TaskFlow.Tests.Auth;

public sealed class PasswordVerifierTests
{
    [Fact]
    public void Verify_RejectsWrongPassword()
    {
        var hash = PasswordHasher.Hash("correct-password");

        Assert.False(PasswordVerifier.Verify("wrong-password", hash));
    }

    [Theory]
    [InlineData("password")]
    [InlineData("123456")]
    public void Verify_RejectsDemoPlaceholder(string password)
    {
        Assert.False(PasswordVerifier.Verify(password, "DEMO_PASSWORD_HASH"));
    }
}
public static class PasswordVerifier
{
    public static bool Verify(string password, string hash)
    {
        if (hash == "DEMO_PASSWORD_HASH")
        {
            return false; // Reject demo placeholder passwords
        }

        return PasswordHasher.Verify(password, hash);
    }
}