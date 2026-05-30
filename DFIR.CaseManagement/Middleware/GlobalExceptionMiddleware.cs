using System.Text.Json;

namespace DFIR.CaseManagement.Middleware;

/// <summary>
/// Catches any unhandled exception and returns a consistent JSON error payload instead
/// of an HTML error page. Maps common domain exceptions to appropriate status codes.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad request"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Bad request"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning("Handled {Exception} on {Method} {Path}: {Message}",
                ex.GetType().Name, context.Request.Method, context.Request.Path, ex.Message);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status,
            title,
            detail = ex.Message,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
