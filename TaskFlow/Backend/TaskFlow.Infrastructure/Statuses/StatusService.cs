using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Statuses;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Statuses;

public sealed class StatusService(TaskFlowDbContext dbContext) : IStatusService
{
    public async Task<StatusResponse> CreateAsync(CreateStatusRequest request, CancellationToken cancellationToken = default)
    {
        var status = new TaskFlow.Domain.Entities.TaskStatus
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsCompleted = request.IsCompleted
        };

        dbContext.TaskStatuses.Add(status);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(status);
    }

    public async Task<IReadOnlyList<StatusResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.TaskStatuses
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Select(s => Map(s))
            .ToListAsync(cancellationToken);
    }

    public async Task<StatusResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.TaskStatuses
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => Map(s))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<StatusResponse?> UpdateAsync(int id, UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        var status = await dbContext.TaskStatuses.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (status is null)
        {
            return null;
        }

        status.Name = request.Name.Trim();
        status.Description = request.Description;
        status.DisplayOrder = request.DisplayOrder;
        status.IsCompleted = request.IsCompleted;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(status);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var status = await dbContext.TaskStatuses.SingleOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (status is null)
        {
            return false;
        }

        dbContext.TaskStatuses.Remove(status);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static StatusResponse Map(TaskFlow.Domain.Entities.TaskStatus status)
    {
        return new StatusResponse(status.Id, status.Name, status.Description, status.DisplayOrder, status.IsCompleted);
    }
}
