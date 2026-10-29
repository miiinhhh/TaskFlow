namespace TaskFlow.Application.Tasks;

public interface ITaskService
{
    Task<TaskResponse?> CreateAsync(CreateTaskRequest request, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskResponse>> GetAllAsync(int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<TaskResponse?> GetByIdAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
}
