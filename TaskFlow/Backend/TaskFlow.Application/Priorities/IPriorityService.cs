namespace TaskFlow.Application.Priorities;

public interface IPriorityService
{
    Task<PriorityResponse> CreateAsync(CreatePriorityRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PriorityResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PriorityResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PriorityResponse?> UpdateAsync(int id, UpdatePriorityRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
