namespace JuniorCodeCRM.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Настройка порядка middleware (используется в Program.cs)
    /// </summary>
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        // Порядок важен:
        // 1. Обработка исключений (самый внешний)
        // 2. Логирование запросов
        // 3. Проверка соединения с БД
        // Затем идут остальные middleware (CORS, маршрутизация, контроллеры)

        app.UseMiddleware<Middleware.ExceptionHandlingMiddleware>();
        app.UseMiddleware<Middleware.RequestLoggingMiddleware>();
        app.UseMiddleware<Middleware.ConnectionCheckMiddleware>();

        return app;
    }
}