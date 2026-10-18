using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Exceptions;
using TaskFlow.Application.Tasks;

namespace TaskFlow.API.Middleware;

/// <summary>
/// Middleware xử lý exception toàn cục, chuẩn hóa JSON lỗi về định dạng thống nhất:
/// { success, statusCode, message, errors, traceId }
///
/// Bảng ánh xạ exception → HTTP status:
///   400 – BadRequestException / TaskValidationException / ValidationException (FluentValidation)
///   401 – UnauthorizedException
///   403 – ForbiddenException / UnauthorizedAccessException
///   404 – NotFoundException
///   409 – ConflictException
///   500 – DbUpdateException / Exception bất kỳ không được xử lý
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            // Nếu response đã bắt đầu ghi (streaming), chỉ có thể re-throw.
            if (context.Response.HasStarted)
            {
                throw;
            }

            logger.LogError(
                exception,
                "Unhandled exception. Method: {Method}, Path: {Path}, TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            await WriteErrorResponseAsync(context, exception);
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, message, errors) = MapException(exception);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            statusCode,
            message,
            errors,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }

    /// <summary>
    /// Ánh xạ exception sang (statusCode, message, errors).
    /// </summary>
    private static (int StatusCode, string Message, object? Errors) MapException(Exception exception)
    {
        return exception switch
        {
            // ── 400 Bad Request ────────────────────────────────────────────
            BadRequestException ex =>
                (400, ex.Message, (object?)null),

            TaskValidationException ex =>
                (400, ex.Message, (object?)null),

            // ── 401 Unauthorized ───────────────────────────────────────────
            UnauthorizedException ex =>
                (401, ex.Message, (object?)null),

            // ── 403 Forbidden ──────────────────────────────────────────────
            ForbiddenException ex =>
                (403, ex.Message, (object?)null),

            UnauthorizedAccessException =>
                (403, "You do not have permission to access this resource.", (object?)null),

            // ── 404 Not Found ──────────────────────────────────────────────
            NotFoundException ex =>
                (404, ex.Message, (object?)null),

            // ── 409 Conflict ───────────────────────────────────────────────
            ConflictException ex =>
                (409, ex.Message, (object?)null),

            // ── 500 – Lỗi database ─────────────────────────────────────────
            // DbUpdateException thường do vi phạm khóa ngoại, unique constraint, v.v.
            DbUpdateException dbEx =>
                (500, BuildDbUpdateMessage(dbEx), (object?)null),

            // ── 500 – Lỗi không xác định ───────────────────────────────────
            _ =>
                (500, "An unexpected error occurred. Please try again later.", (object?)null)
        };
    }

    /// <summary>
    /// Chuyển DbUpdateException thành thông báo thân thiện.
    /// Không lộ chi tiết nội bộ (connection string, SQL…) ra ngoài.
    /// </summary>
    private static string BuildDbUpdateMessage(DbUpdateException ex)
    {
        // Lấy inner-most message để phát hiện loại lỗi DB phổ biến.
        var inner = ex.InnerException?.Message ?? string.Empty;

        if (inner.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
            inner.Contains("foreign key", StringComparison.OrdinalIgnoreCase))
        {
            return "A database operation failed: a referenced record does not exist.";
        }

        if (inner.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
            inner.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
        {
            return "A database operation failed: a duplicate record already exists.";
        }

        return "A database operation failed. Please check your input and try again.";
    }
}
