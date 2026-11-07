using System.Diagnostics;

namespace Churn.API.Middleware;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();
        try { await next(context); }
        finally
        {
            timer.Stop();
            logger.LogInformation("{Method} {Path} returned {StatusCode} in {ElapsedMs} ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, timer.ElapsedMilliseconds);
        }
    }
}
