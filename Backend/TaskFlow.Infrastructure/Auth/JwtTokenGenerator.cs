using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TaskFlow.Application.Auth;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Infrastructure.Auth;

internal static class JwtTokenGenerator
{
    public static string CreateToken(AuthUserResponse user, DateTime expiresAt, IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"]
            ?? "TaskFlow development secret key. Replace this value in appsettings for production.";

        var issuer = configuration["Jwt:Issuer"] ?? "TaskFlow";
        var audience = configuration["Jwt:Audience"] ?? "TaskFlow";
        var now = DateTimeOffset.UtcNow;

        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object?>
        {
            [ClaimTypes.NameIdentifier] = user.Id.ToString(),
            [ClaimTypes.Name] = user.Username,
            [ClaimTypes.Email] = user.Email,
            [ClaimTypes.Role] = user.RoleName,
            ["fullName"] = user.FullName,
            ["roleId"] = user.RoleId,
            ["iss"] = issuer,
            ["aud"] = audience,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["exp"] = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
        };

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var unsignedToken = $"{encodedHeader}.{encodedPayload}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signature = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)));

        return $"{unsignedToken}.{signature}";
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
