using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Auth;
using TaskFlow.Application.Tasks;
using TaskFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var auth = app.MapGroup("/api/auth")
    .WithTags("Auth");

auth.MapPost("/login", async (
    LoginRequest request,
    IAuthService authService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest("Username/email and password are required.");
    }

    var login = await authService.LoginAsync(request, cancellationToken);
    return login is null ? Results.Unauthorized() : Results.Ok(login);
})
.WithName("Login");

var tasks = app.MapGroup("/api/tasks")
    .WithTags("Tasks");

tasks.MapPost("/", async (
    CreateTaskRequest request,
    ITaskService taskService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title is required.");
    }

    try
    {
        var createdTask = await taskService.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/tasks/{createdTask.Id}", createdTask);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Project, creator, status, or priority does not exist.");
    }
})
.WithName("CreateTask");

tasks.MapGet("/", async (ITaskService taskService, CancellationToken cancellationToken) =>
{
    var taskList = await taskService.GetAllAsync(cancellationToken);
    return Results.Ok(taskList);
})
.WithName("GetTasks");

tasks.MapGet("/{id:int}", async (int id, ITaskService taskService, CancellationToken cancellationToken) =>
{
    var task = await taskService.GetByIdAsync(id, cancellationToken);
    return task is null ? Results.NotFound() : Results.Ok(task);
})
.WithName("GetTaskById");

tasks.MapPut("/{id:int}", async (
    int id,
    UpdateTaskRequest request,
    ITaskService taskService,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest("Title is required.");
    }

    try
    {
        var updatedTask = await taskService.UpdateAsync(id, request, cancellationToken);
        return updatedTask is null ? Results.NotFound() : Results.Ok(updatedTask);
    }
    catch (DbUpdateException)
    {
        return Results.BadRequest("Project, status, or priority does not exist.");
    }
})
.WithName("UpdateTask");

tasks.MapDelete("/{id:int}", async (int id, ITaskService taskService, CancellationToken cancellationToken) =>
{
    var deleted = await taskService.DeleteAsync(id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteTask");

app.Run();
