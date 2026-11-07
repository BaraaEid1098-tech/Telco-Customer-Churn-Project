using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Churn.API.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error processing {Method} {Path}", context.Request.Method, context.Request.Path);
            if (context.Response.HasStarted) throw;
            context.Response.Clear();
            context.Response.StatusCode = ex switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status409Conflict,
                HttpRequestException => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status500InternalServerError
            };
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = context.Response.StatusCode,
                Title = context.Response.StatusCode == 502 ? "Prediction service unavailable" : "Request failed",
                Detail = context.Response.StatusCode < 500 ? ex.Message : "The request could not be completed.",
                Instance = context.Request.Path
            });
        }
    }
}
