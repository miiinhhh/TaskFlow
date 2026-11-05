using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Projects;

namespace TaskFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
[Produces("application/json")]
public class TaskAssignmentsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public TaskAssignmentsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPut("{taskId:int}/assignment")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TaskAssignmentResponse>> Assign(
        int taskId,
        AssignTaskRequest request,
        CancellationToken cancellationToken)
    {
        var assignment = await _projectService.AssignTaskAsync(
            taskId,
            request.UserId,
            GetCurrentUserId(),
            IsAdmin(),
            cancellationToken);

        if (assignment == null)
        {
            return BadRequest("Task does not exist, or assignee is not a project member.");
        }

        return Ok(assignment);
    }

    [HttpDelete("{taskId:int}/assignment")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(int taskId, CancellationToken cancellationToken)
    {
        bool removed = await _projectService.UnassignTaskAsync(
            taskId,
            GetCurrentUserId(),
            IsAdmin(),
            cancellationToken);

        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{taskId:int}/assignment/{userId:int}")]
    public async Task<IActionResult> IsAssigned(int taskId, int userId, CancellationToken cancellationToken)
    {
        bool isAssigned = await _projectService.IsAssignedAsync(
            taskId,
            userId,
            GetCurrentUserId(),
            IsAdmin(),
            cancellationToken);

        return Ok(new
        {
            taskId = taskId,
            userId = userId,
            isAssigned = isAssigned
        });
    }

    private int GetCurrentUserId()
    {
        string? value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int id;
        if (int.TryParse(value, out id))
        {
            return id;
        }

        return 0;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }
}
