using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TaskFlow.Application.Auth;

namespace TaskFlow.Infrastructure.Auth;

internal static class JwtTokenGenerator
{
    public static string CreateToken(AuthUserResponse user, DateTime expiresAt, IConfiguration configuration)
    {
        string? secret = configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("Jwt:Secret is not configured.");
        }

        string issuer = configuration["Jwt:Issuer"] ?? "TaskFlow";
        string audience = configuration["Jwt:Audience"] ?? "TaskFlow";

        DateTimeOffset now = DateTimeOffset.UtcNow;

        var header = new Dictionary<string, object>();
        header["alg"] = "HS256";
        header["typ"] = "JWT";

        var payload = new Dictionary<string, object?>();
        payload[ClaimTypes.NameIdentifier] = user.Id.ToString();
        payload[ClaimTypes.Name] = user.Username;
        payload[ClaimTypes.Email] = user.Email;
        payload[ClaimTypes.Role] = user.RoleName;
        payload["fullName"] = user.FullName;
        payload["roleId"] = user.RoleId;
        payload["iss"] = issuer;
        payload["aud"] = audience;
        payload["iat"] = now.ToUnixTimeSeconds();
        payload["exp"] = new DateTimeOffset(expiresAt).ToUnixTimeSeconds();

        string encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        string encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        string unsignedToken = encodedHeader + "." + encodedPayload;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        string signature = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)));

        return unsignedToken + "." + signature;
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        string text = Convert.ToBase64String(bytes);
        text = text.TrimEnd('=');
        text = text.Replace('+', '-');
        text = text.Replace('/', '_');
        return text;
    }
}
