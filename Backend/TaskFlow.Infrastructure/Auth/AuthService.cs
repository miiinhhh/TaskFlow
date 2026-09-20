using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskFlow.Application.Auth;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Auth;

public sealed class AuthService(TaskFlowDbContext dbContext, IConfiguration configuration) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var usernameOrEmail = request.UsernameOrEmail.Trim();

        var user = await dbContext.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .SingleOrDefaultAsync(
                u => u.IsActive && (u.Username == usernameOrEmail || u.Email == usernameOrEmail),
                cancellationToken);

        if (user is null || !PasswordVerifier.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var expiresAt = DateTime.UtcNow.AddHours(GetTokenLifetimeHours());
        var authUser = new AuthUserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.AvatarUrl,
            user.RoleId,
            user.Role?.Name);

        var token = JwtTokenGenerator.CreateToken(authUser, expiresAt, configuration);

        return new LoginResponse(token, "Bearer", expiresAt, authUser);
    }

    private int GetTokenLifetimeHours()
    {
        return int.TryParse(configuration["Jwt:ExpiresHours"], out var hours) && hours > 0
            ? hours
            : 8;
    }
}
