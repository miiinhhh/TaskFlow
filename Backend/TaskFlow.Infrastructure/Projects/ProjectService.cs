using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Projects;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Projects;

public sealed class ProjectService(TaskFlowDbContext dbContext) : IProjectService
{
    public async Task<ProjectResponse> CreateAsync(int currentUserId, CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            OwnerId = currentUserId,
            Name = request.Name.Trim(),
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = currentUserId,
            JoinedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ProjectQuery().SingleAsync(p => p.Id == project.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectResponse>> GetAllAsync(int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var query = ProjectQuery();
        if (!isAdmin)
        {
            query = query.Where(p => dbContext.ProjectMembers.Any(m => m.ProjectId == p.Id && m.UserId == currentUserId));
        }

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task<ProjectResponse?> GetByIdAsync(int projectId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        return ProjectQuery()
            .Where(p => p.Id == projectId && (isAdmin || dbContext.ProjectMembers.Any(m => m.ProjectId == projectId && m.UserId == currentUserId)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ProjectResponse?> UpdateAsync(int projectId, int currentUserId, bool isAdmin, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await GetManagedProjectAsync(projectId, currentUserId, isAdmin, cancellationToken);
        if (project is null)
        {
            return null;
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.IsArchived = request.IsArchived;
        project.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await ProjectQuery().SingleAsync(p => p.Id == projectId, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int projectId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var project = await GetManagedProjectAsync(projectId, currentUserId, isAdmin, cancellationToken);
        if (project is null)
        {
            return false;
        }

        project.IsArchived = true;
        project.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ProjectMemberResponse>?> GetMembersAsync(int projectId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (!await HasProjectAccessAsync(projectId, currentUserId, isAdmin, cancellationToken))
        {
            return null;
        }

        return await dbContext.ProjectMembers
            .AsNoTracking()
            .Where(m => m.ProjectId == projectId)
            .Select(m => new ProjectMemberResponse(
                m.ProjectId,
                m.UserId,
                m.User!.Username,
                m.User.FullName,
                m.User.Role == null ? null : m.User.Role.Name,
                m.JoinedAt))
            .OrderBy(m => m.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AddMemberAsync(int projectId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (await GetManagedProjectAsync(projectId, currentUserId, isAdmin, cancellationToken) is null
            || !await dbContext.Users.AnyAsync(u => u.Id == userId && u.IsActive, cancellationToken)
            || await dbContext.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken))
        {
            return false;
        }

        dbContext.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = userId, JoinedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveMemberAsync(int projectId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var project = await GetManagedProjectAsync(projectId, currentUserId, isAdmin, cancellationToken);
        var membership = await dbContext.ProjectMembers.SingleOrDefaultAsync(
            m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

        if (project is null || membership is null || project.OwnerId == userId)
        {
            return false;
        }

        dbContext.ProjectMembers.Remove(membership);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsMemberAsync(int projectId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (!await HasProjectAccessAsync(projectId, currentUserId, isAdmin, cancellationToken))
        {
            return false;
        }

        return await dbContext.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);
    }

    public async Task<TaskAssignmentResponse?> AssignTaskAsync(int taskId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.Tasks.SingleOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted, cancellationToken);
        if (task is null
            || await GetManagedProjectAsync(task.ProjectId, currentUserId, isAdmin, cancellationToken) is null
            || !await dbContext.ProjectMembers.AnyAsync(m => m.ProjectId == task.ProjectId && m.UserId == userId, cancellationToken))
        {
            return null;
        }

        var oldAssignments = await dbContext.TaskAssignments.Where(a => a.TaskId == taskId).ToListAsync(cancellationToken);
        dbContext.TaskAssignments.RemoveRange(oldAssignments);
        var assignment = new TaskAssignment
        {
            TaskId = taskId,
            UserId = userId,
            AssignedById = currentUserId,
            AssignedAt = DateTime.UtcNow
        };
        dbContext.TaskAssignments.Add(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.TaskAssignments
            .AsNoTracking()
            .Where(a => a.TaskId == taskId && a.UserId == userId)
            .Select(a => new TaskAssignmentResponse(
                a.TaskId,
                a.Task!.ProjectId,
                a.UserId,
                a.User!.Username,
                a.User.FullName,
                a.AssignedById!.Value,
                a.AssignedAt))
            .SingleAsync(cancellationToken);
    }

    public async Task<bool> IsAssignedAsync(int taskId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var projectId = await dbContext.Tasks
            .Where(t => t.Id == taskId && !t.IsDeleted)
            .Select(t => (int?)t.ProjectId)
            .SingleOrDefaultAsync(cancellationToken);

        return projectId.HasValue
            && await HasProjectAccessAsync(projectId.Value, currentUserId, isAdmin, cancellationToken)
            && await dbContext.TaskAssignments.AnyAsync(a => a.TaskId == taskId && a.UserId == userId, cancellationToken);
    }

    private IQueryable<ProjectResponse> ProjectQuery()
    {
        return dbContext.Projects
            .AsNoTracking()
            .Where(p => !p.IsArchived)
            .Select(p => new ProjectResponse(
                p.Id,
                p.OwnerId,
                dbContext.Users.Where(u => u.Id == p.OwnerId).Select(u => u.FullName).SingleOrDefault(),
                p.Name,
                p.Description,
                p.StartDate,
                p.EndDate,
                p.IsArchived,
                p.CreatedAt,
                p.UpdatedAt,
                dbContext.ProjectMembers.Count(m => m.ProjectId == p.Id)));
    }

    private Task<Project?> GetManagedProjectAsync(int projectId, int currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        return dbContext.Projects.SingleOrDefaultAsync(
            p => p.Id == projectId && !p.IsArchived && (isAdmin || p.OwnerId == currentUserId), cancellationToken);
    }

    private Task<bool> HasProjectAccessAsync(int projectId, int currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        return isAdmin
            ? Task.FromResult(true)
            : dbContext.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == currentUserId, cancellationToken);
    }
}