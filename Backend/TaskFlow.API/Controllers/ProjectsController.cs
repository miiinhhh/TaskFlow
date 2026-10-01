using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Projects;

namespace TaskFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
[Produces("application/json")]
public sealed class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 200 || request.EndDate < request.StartDate)
        {
            return BadRequest("Name is required and dates are invalid.");
        }

        var project = await projectService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetAll(CancellationToken cancellationToken)
        => Ok(await projectService.GetAllAsync(CurrentUserId, IsAdmin, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var project = await projectService.GetByIdAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> Update(int id, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 200 || request.EndDate < request.StartDate)
        {
            return BadRequest("Name is required and dates are invalid.");
        }

        var project = await projectService.UpdateAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return project is null ? Forbid() : Ok(project);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await projectService.DeleteAsync(id, CurrentUserId, IsAdmin, cancellationToken) ? NoContent() : Forbid();

    [HttpGet("{id:int}/members")]
    public async Task<ActionResult<IReadOnlyList<ProjectMemberResponse>>> GetMembers(int id, CancellationToken cancellationToken)
    {
        var members = await projectService.GetMembersAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return members is null ? Forbid() : Ok(members);
    }

    [HttpPost("{id:int}/members")]
    public async Task<IActionResult> AddMember(int id, AddProjectMemberRequest request, CancellationToken cancellationToken)
        => await projectService.AddMemberAsync(id, request.UserId, CurrentUserId, IsAdmin, cancellationToken)
            ? NoContent()
            : BadRequest("Project, user, or membership is invalid.");

    [HttpDelete("{id:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveMember(int id, int userId, CancellationToken cancellationToken)
        => await projectService.RemoveMemberAsync(id, userId, CurrentUserId, IsAdmin, cancellationToken)
            ? NoContent()
            : BadRequest("Project, membership, or owner constraint is invalid.");

    [HttpGet("{id:int}/members/{userId:int}")]
    public async Task<IActionResult> IsMember(int id, int userId, CancellationToken cancellationToken)
        => Ok(new { projectId = id, userId, isMember = await projectService.IsMemberAsync(id, userId, CurrentUserId, IsAdmin, cancellationToken) });

    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
    private bool IsAdmin => User.IsInRole("Admin");
}