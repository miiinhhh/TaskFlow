using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Priorities;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/priorities")]
[Produces("application/json")]
public sealed class PrioritiesController(IPriorityService priorityService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PriorityResponse>> Create(CreatePriorityRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        try
        {
            var priority = await priorityService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = priority.Id }, priority);
        }
        catch (DbUpdateException)
        {
            return BadRequest("Priority name already exists.");
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
    public async Task<ActionResult<PriorityResponse>> Update(int id, UpdatePriorityRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        try
        {
            var priority = await priorityService.UpdateAsync(id, request, cancellationToken);
            return priority is null ? NotFound() : Ok(priority);
        }
        catch (DbUpdateException)
        {
            return BadRequest("Priority name already exists or priority is being used.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await priorityService.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (DbUpdateException)
        {
            return BadRequest("Priority is being used by tasks.");
        }
    }
}
