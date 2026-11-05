using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Priorities;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Priorities;

public sealed class PriorityService(TaskFlowDbContext dbContext) : IPriorityService
{
    public async Task<PriorityResponse> CreateAsync(CreatePriorityRequest request, CancellationToken cancellationToken = default)
    {
        var priority = new TaskPriority
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            Level = request.Level
        };

        dbContext.TaskPriorities.Add(priority);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(priority);
    }

    public async Task<IReadOnlyList<PriorityResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.TaskPriorities
            .AsNoTracking()
            .OrderBy(p => p.Level)
            .ThenBy(p => p.Name)
            .Select(p => Map(p))
            .ToListAsync(cancellationToken);
    }

    public async Task<PriorityResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.TaskPriorities
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => Map(p))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PriorityResponse?> UpdateAsync(int id, UpdatePriorityRequest request, CancellationToken cancellationToken = default)
    {
        var priority = await dbContext.TaskPriorities.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (priority is null)
        {
            return null;
        }

        priority.Name = request.Name.Trim();
        priority.Description = request.Description;
        priority.Level = request.Level;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(priority);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var priority = await dbContext.TaskPriorities.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (priority is null)
        {
            return false;
        }

        dbContext.TaskPriorities.Remove(priority);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PriorityResponse Map(TaskPriority priority)
    {
        return new PriorityResponse(priority.Id, priority.Name, priority.Description, priority.Level);
    }
}
