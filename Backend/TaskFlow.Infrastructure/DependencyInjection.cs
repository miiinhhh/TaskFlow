using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Auth;
using TaskFlow.Application.Comments;
using TaskFlow.Application.Histories;
using TaskFlow.Application.Priorities;
using TaskFlow.Application.Tasks;
using TaskFlow.Application.Projects;
using TaskFlow.Application.Statuses;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Auth;
using TaskFlow.Infrastructure.Comments;
using TaskFlow.Infrastructure.Histories;
using TaskFlow.Infrastructure.Priorities;
using TaskFlow.Infrastructure.Tasks;
using TaskFlow.Infrastructure.Projects;
using TaskFlow.Infrastructure.Statuses;

namespace TaskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaskFlowDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStatusService, StatusService>();
        services.AddScoped<IPriorityService, PriorityService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ITaskHistoryService, TaskHistoryService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IProjectService, ProjectService>();

        return services;
    }
}
