using System.Diagnostics;

namespace TaskFlow.API.Middleware;

public sealed class RequestTraceMiddleware(
    RequestDelegate next,
    ILogger<RequestTraceMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation(
            "Request started {Method} {Path} TraceId={TraceId}",
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation(
                "Request completed {Method} {Path} StatusCode={StatusCode} ElapsedMs={ElapsedMs} TraceId={TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                context.TraceIdentifier);
        }
    }
}
