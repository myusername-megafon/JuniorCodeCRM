using JuniorCodeCRM.Helpers;
using System.Net;

namespace JuniorCodeCRM.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

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
        catch (ArgumentException ex)
        {
            // Ошибки валидации — 400 Bad Request
            _logger.LogWarning(ex, "Ошибка валидации данных");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(JsonHelper.Serialize(new
            {
                error = ex.Message,
                type = "ValidationError"
            }));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase))
        {
            // Потеря соединения с БД — 503 Service Unavailable
            _logger.LogError(ex, "Ошибка соединения с базой данных");
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(JsonHelper.Serialize(new
            {
                error = "Нет соединения с сервером. Попробуйте позже.",
                type = "ConnectionError"
            }));
        }
        catch (Exception ex)
        {
            // Все остальные ошибки — 500 Internal Server Error
            _logger.LogError(ex, "Необработанное исключение");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(JsonHelper.Serialize(new
            {
                error = "Внутренняя ошибка сервера",
                type = "InternalError"
            }));
        }
    }
}