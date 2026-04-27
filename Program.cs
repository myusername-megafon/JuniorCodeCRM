using JuniorCodeCRM.Data;
using JuniorCodeCRM.Extensions;
using JuniorCodeCRM.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// База данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация всех сервисов
builder.Services.AddApplicationServices();
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// CORS для веб-клиента и мобильного приложения
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Раздача статических файлов из wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// SPA fallback — все не-API запросы направляем на index.html
app.MapWhen(ctx => !ctx.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        if (!System.IO.Path.HasExtension(context.Request.Path.Value))
        {
            context.Request.Path = "/index.html";
        }
        await next();
    });
    appBuilder.UseStaticFiles();
});

app.UseHttpsRedirection();
app.MapControllers();

// Инициализация БД
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Initialize(context);
}

app.Run();