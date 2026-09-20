using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskFlow.Application.Auth;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Auth;

public sealed class AuthService(TaskFlowDbContext dbContext, IConfiguration configuration) : IAuthService
{
    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim();
        var fullName = request.FullName.Trim();

        var usernameExists = await dbContext.Users
            .AnyAsync(u => u.Username == username, cancellationToken);

        if (usernameExists)
        {
            return new RegisterResult(false, "Username already exists.", null);
        }

        var emailExists = await dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailExists)
        {
            return new RegisterResult(false, "Email already exists.", null);
        }

        var roleId = request.RoleId ?? await GetMemberRoleIdAsync(cancellationToken);
        var roleExists = await dbContext.Roles
            .AnyAsync(r => r.Id == roleId, cancellationToken);

        if (!roleExists)
        {
            return new RegisterResult(false, "Role does not exist.", null);
        }

        var user = new User
        {
            RoleId = roleId,
            Username = username,
            Email = email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            FullName = fullName,
            AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        var login = await LoginAsync(new LoginRequest(username, request.Password), cancellationToken);
        return new RegisterResult(true, null, login);
    }

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

    private async Task<int> GetMemberRoleIdAsync(CancellationToken cancellationToken)
    {
        var memberRoleId = await dbContext.Roles
            .Where(r => r.Name == "Member")
            .Select(r => (int?)r.Id)
            .SingleOrDefaultAsync(cancellationToken);

        return memberRoleId ?? 3;
    }
}
