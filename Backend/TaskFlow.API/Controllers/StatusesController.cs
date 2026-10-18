using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Statuses;

namespace TaskFlow.API.Controllers;

[ApiController]
[Route("api/statuses")]
[Produces("application/json")]
public sealed class StatusesController(IStatusService statusService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<StatusResponse>> Create(CreateStatusRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var status = await statusService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = status.Id }, status);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StatusResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await statusService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StatusResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var status = await statusService.GetByIdAsync(id, cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StatusResponse>> Update(int id, UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Name is required.");
        }

        var status = await statusService.UpdateAsync(id, request, cancellationToken);
        return status is null ? NotFound() : Ok(status);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await statusService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
