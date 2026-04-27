namespace JuniorCodeCRM.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var method = context.Request.Method;
        var path = context.Request.Path;
        var ip = context.Connection.RemoteIpAddress?.ToString();

        await _next(context);

        var elapsed = DateTime.UtcNow - startTime;
        var statusCode = context.Response.StatusCode;

        if (statusCode >= 400)
        {
            _logger.LogWarning(
                "[{IP}] {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                ip, method, path, statusCode, elapsed.TotalMilliseconds);
        }
        else
        {
            _logger.LogInformation(
                "[{IP}] {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                ip, method, path, statusCode, elapsed.TotalMilliseconds);
        }
    }
}