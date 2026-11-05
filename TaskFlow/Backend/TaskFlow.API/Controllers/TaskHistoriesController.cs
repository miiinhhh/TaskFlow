using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Histories;

namespace TaskFlow.API.Controllers;

[ApiController]
[Produces("application/json")]
public sealed class TaskHistoriesController(ITaskHistoryService historyService) : ControllerBase
{
    [HttpPost("api/tasks/{taskId:int}/histories")]
    public async Task<ActionResult<TaskHistoryResponse>> Create(
        int taskId,
        CreateTaskHistoryRequest request,
        CancellationToken cancellationToken)
    {
        if (taskId != request.TaskId)
        {
            return BadRequest("Route task id must match request task id.");
        }

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            return BadRequest("Action is required.");
        }

        try
        {
            var history = await historyService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = history.Id }, history);
        }
        catch (DbUpdateException)
        {
            return BadRequest("Task or user does not exist.");
        }
    }

    [HttpGet("api/tasks/{taskId:int}/histories")]
    public async Task<ActionResult<IReadOnlyList<TaskHistoryResponse>>> GetByTaskId(int taskId, CancellationToken cancellationToken)
    {
        return Ok(await historyService.GetByTaskIdAsync(taskId, cancellationToken));
    }

    [HttpGet("api/task-histories/{id:int}")]
    public async Task<ActionResult<TaskHistoryResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var history = await historyService.GetByIdAsync(id, cancellationToken);
        return history is null ? NotFound() : Ok(history);
    }
}
