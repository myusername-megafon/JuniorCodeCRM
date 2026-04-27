using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.DTOs.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Логин обязателен")]
    [MaxLength(50)]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен")]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}