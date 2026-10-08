namespace TaskFlow.Application.Histories;

public interface ITaskHistoryService
{
    Task<TaskHistoryResponse> CreateAsync(CreateTaskHistoryRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskHistoryResponse>> GetByTaskIdAsync(int taskId, CancellationToken cancellationToken = default);
    Task<TaskHistoryResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
