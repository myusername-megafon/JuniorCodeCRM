using JuniorCodeCRM.Helpers;
using JuniorCodeCRM.Models.DTOs.Auth;
using JuniorCodeCRM.Models.Enums;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace JuniorCodeCRM.Services;

public class AuthService : IAuthService
{
    private readonly string _passwordHash;
    private readonly IAuditService _auditService;

    public AuthService(IConfiguration configuration, IAuditService auditService)
    {
        _auditService = auditService;

        // Читаем открытый пароль из конфига и сразу хешируем
        var plainPassword = configuration["Auth:Password"];
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            plainPassword = "admin123"; // пароль по умолчанию
        }

        _passwordHash = PasswordHasher.Hash(plainPassword);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            await _auditService.LogAsync(ActionType.LOGIN, EntityType.Employee, null,
                null, "Попытка входа с пустыми полями", ipAddress);
            return new LoginResponse
            {
                Success = false,
                Message = "Логин и пароль обязательны"
            };
        }

        if (!ValidatePassword(request.Password))
        {
            await _auditService.LogAsync(ActionType.LOGIN, EntityType.Employee, null,
                null, $"Неудачная попытка входа: {request.Login}", ipAddress);
            return new LoginResponse
            {
                Success = false,
                Message = "Неверный логин или пароль"
            };
        }

        await _auditService.LogAsync(ActionType.LOGIN, EntityType.Employee, null,
            null, $"Успешный вход: {request.Login}", ipAddress);

        return new LoginResponse
        {
            Success = true,
            Message = "Вход выполнен успешно",
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.Now.AddHours(8)
        };
    }

    public bool ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(_passwordHash))
            return password == "admin123"; // fallback

        return PasswordHasher.Verify(password, _passwordHash);
    }
}