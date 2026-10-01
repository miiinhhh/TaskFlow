namespace TaskFlow.Application.Projects;

public interface IProjectService
{
    Task<ProjectResponse> CreateAsync(int currentUserId, CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectResponse>> GetAllAsync(int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<ProjectResponse?> GetByIdAsync(int projectId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<ProjectResponse?> UpdateAsync(int projectId, int currentUserId, bool isAdmin, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int projectId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectMemberResponse>?> GetMembersAsync(int projectId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> AddMemberAsync(int projectId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> RemoveMemberAsync(int projectId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(int projectId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<TaskAssignmentResponse?> AssignTaskAsync(int taskId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> IsAssignedAsync(int taskId, int userId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
}