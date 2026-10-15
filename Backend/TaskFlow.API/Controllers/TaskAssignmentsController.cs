using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Projects;

namespace TaskFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
[Produces("application/json")]
public sealed class TaskAssignmentsController(IProjectService projectService) : ControllerBase
{
    [HttpPut("{taskId:int}/assignment")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TaskAssignmentResponse>> Assign(
        int taskId,
        AssignTaskRequest request,
        CancellationToken cancellationToken)
    {
        var assignment = await projectService.AssignTaskAsync(
            taskId, request.UserId, CurrentUserId, IsAdmin, cancellationToken);

        return assignment is null
            ? BadRequest("Task does not exist, or assignee is not a project member.")
            : Ok(assignment);
    }

    [HttpDelete("{taskId:int}/assignment")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unassign(int taskId, CancellationToken cancellationToken)
    {
        var removed = await projectService.UnassignTaskAsync(
            taskId, CurrentUserId, IsAdmin, cancellationToken);

        return removed ? NoContent() : NotFound();
    }

    [HttpGet("{taskId:int}/assignment/{userId:int}")]
    public async Task<IActionResult> IsAssigned(int taskId, int userId, CancellationToken cancellationToken)
        => Ok(new
        {
            taskId,
            userId,
            isAssigned = await projectService.IsAssignedAsync(
                taskId, userId, CurrentUserId, IsAdmin, cancellationToken)
        });

    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private bool IsAdmin => User.IsInRole("Admin");
}
