namespace TaskFlow.Application.Statuses;

public interface IStatusService
{
    Task<StatusResponse> CreateAsync(CreateStatusRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatusResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StatusResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StatusResponse?> UpdateAsync(int id, UpdateStatusRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
