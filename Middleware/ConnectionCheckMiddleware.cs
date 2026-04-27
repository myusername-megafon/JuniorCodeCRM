using JuniorCodeCRM.Data;
using Microsoft.EntityFrameworkCore;

namespace JuniorCodeCRM.Middleware;

public class ConnectionCheckMiddleware
{
    private readonly RequestDelegate _next;

    public ConnectionCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        try
        {
            // Проверяем соединение с БД (с таймаутом 3 секунды)
            var canConnect = await dbContext.Database.CanConnectAsync(
                context.RequestAborted);

            if (!canConnect)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(
                    "{\"error\":\"Нет соединения с сервером базы данных\"}");
                return;
            }
        }
        catch (Exception)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(
                "{\"error\":\"Нет соединения с сервером\"}");
            return;
        }

        await _next(context);
    }
}