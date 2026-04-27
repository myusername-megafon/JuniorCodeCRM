namespace JuniorCodeCRM.Models.DTOs.Auth;

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }              // JWT-токен (если используется)
    public DateTime? ExpiresAt { get; set; }
}