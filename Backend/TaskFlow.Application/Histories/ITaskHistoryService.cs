namespace TaskFlow.Application.Histories;

public interface ITaskHistoryService
{
    Task<TaskHistoryResponse?> CreateAsync(int taskId, int currentUserId, bool isAdmin, CreateTaskHistoryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskHistoryResponse>?> GetByTaskIdAsync(int taskId, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<TaskHistoryResponse?> GetByIdAsync(int id, int currentUserId, bool isAdmin, CancellationToken cancellationToken = default);
}
