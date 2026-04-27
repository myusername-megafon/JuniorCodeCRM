using JuniorCodeCRM.Models.DTOs.Auth;

namespace JuniorCodeCRM.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string? ipAddress);
    bool ValidatePassword(string password);
}