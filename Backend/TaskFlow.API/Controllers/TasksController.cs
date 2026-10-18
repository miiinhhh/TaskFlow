using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskFlow.Application.Tasks;

namespace TaskFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
[Produces("application/json")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId <= 0)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Title)
            || request.Title.Length > 200
            || request.EstimatedHours is < 0
            || request.ActualHours is < 0
            || (request.StartDate.HasValue && request.DueDate.HasValue && request.DueDate < request.StartDate))
        {
            return BadRequest("Title is required and valid. Hours cannot be negative, and due date must not precede start date.");
        }

        var createdTask = await taskService.CreateAsync(request, CurrentUserId, IsAdmin, cancellationToken);
        if (createdTask is null)
        {
            return Forbid();
        }

        return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var taskList = await taskService.GetAllAsync(CurrentUserId, IsAdmin, cancellationToken);
        return Ok(taskList);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await taskService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(
        int id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId <= 0)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Title)
            || request.Title.Length > 200
            || request.EstimatedHours is < 0
            || request.ActualHours is < 0
            || (request.StartDate.HasValue && request.DueDate.HasValue && request.DueDate < request.StartDate))
        {
            return BadRequest("Title is required and valid. Hours cannot be negative, and due date must not precede start date.");
        }

        var updatedTask = await taskService.UpdateAsync(id, request, CurrentUserId, IsAdmin, cancellationToken);
        return updatedTask is null ? NotFound() : Ok(updatedTask);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await taskService.DeleteAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private bool IsAdmin => User.IsInRole("Admin");
}
