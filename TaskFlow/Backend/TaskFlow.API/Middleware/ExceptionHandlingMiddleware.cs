using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Exceptions;
using TaskFlow.Application.Tasks;

namespace TaskFlow.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            _logger.LogError(
                exception,
                "Unhandled exception. Method: {Method}, Path: {Path}, TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteErrorResponseAsync(context, exception);
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string message;
        GetErrorInfo(exception, out statusCode, out message);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            statusCode = statusCode,
            message = message,
            errors = (object?)null,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static void GetErrorInfo(Exception exception, out int statusCode, out string message)
    {
        if (exception is BadRequestException || exception is TaskValidationException)
        {
            statusCode = 400;
            message = exception.Message;
            return;
        }

        if (exception is UnauthorizedException)
        {
            statusCode = 401;
            message = exception.Message;
            return;
        }

        if (exception is ForbiddenException)
        {
            statusCode = 403;
            message = exception.Message;
            return;
        }

        if (exception is UnauthorizedAccessException)
        {
            statusCode = 403;
            message = "You do not have permission to access this resource.";
            return;
        }

        if (exception is NotFoundException)
        {
            statusCode = 404;
            message = exception.Message;
            return;
        }

        if (exception is ConflictException)
        {
            statusCode = 409;
            message = exception.Message;
            return;
        }

        if (exception is DbUpdateException dbEx)
        {
            statusCode = 500;
            message = BuildDbUpdateMessage(dbEx);
            return;
        }

        statusCode = 500;
        message = "An unexpected error occurred. Please try again later.";
    }

    private static string BuildDbUpdateMessage(DbUpdateException ex)
    {
        string inner = string.Empty;
        if (ex.InnerException != null)
        {
            inner = ex.InnerException.Message;
        }

        if (inner.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase)
            || inner.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
        {
            return "A database operation failed: a referenced record does not exist.";
        }

        if (inner.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
            || inner.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
        {
            return "A database operation failed: a duplicate record already exists.";
        }

        return "A database operation failed. Please check your input and try again.";
    }
}
