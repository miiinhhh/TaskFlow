using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskFlow.Application.Auth;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Tests.Auth;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task Register_AssignsMemberRole()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Roles.AddRange(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Manager" },
            new Role { Id = 3, Name = "Member" });
        await dbContext.SaveChangesAsync();
        var service = new AuthService(dbContext, CreateConfiguration());

        var result = await service.RegisterAsync(new RegisterRequest(
            "new-member",
            "member@example.com",
            "secure-password",
            "New Member",
            null));

        Assert.True(result.Succeeded);
        var user = await dbContext.Users.SingleAsync();
        Assert.Equal(3, user.RoleId);
        Assert.Equal("Member", result.Login?.User.RoleName);
    }

    [Fact]
    public async Task Login_RejectsWrongPassword()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Roles.Add(new Role { Id = 3, Name = "Member" });
        dbContext.Users.Add(new User
        {
            Id = 1,
            RoleId = 3,
            Username = "member",
            Email = "member@example.com",
            PasswordHash = PasswordHasher.Hash("correct-password"),
            FullName = "Member",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
        var service = new AuthService(dbContext, CreateConfiguration());

        var login = await service.LoginAsync(new LoginRequest("member", "wrong-password"));

        Assert.Null(login);
    }

    private static TaskFlowDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TaskFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TaskFlowDbContext(options);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "TaskFlow.Tests",
                ["Jwt:Audience"] = "TaskFlow.Tests",
                ["Jwt:Secret"] = "A test-only secret long enough for HMAC SHA-256 signing.",
                ["Jwt:ExpiresHours"] = "1"
            })
            .Build();
    }
}
