using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Exceptions;
using TaskFlow.Application.Priorities;

namespace TaskFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/priorities")]
[Produces("application/json")]
public sealed class PrioritiesController(IPriorityService priorityService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PriorityResponse>> Create(CreatePriorityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var priority = await priorityService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = priority.Id }, priority);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Priority name already exists.");
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PriorityResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await priorityService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PriorityResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var priority = await priorityService.GetByIdAsync(id, cancellationToken);
        return priority is null ? NotFound() : Ok(priority);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PriorityResponse>> Update(int id, UpdatePriorityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var priority = await priorityService.UpdateAsync(id, request, cancellationToken);
            return priority is null ? NotFound() : Ok(priority);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Priority name already exists or the priority is being used.");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await priorityService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Priority is being used by tasks.");
        }
    }
}
