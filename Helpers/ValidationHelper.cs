using System.Text.RegularExpressions;

namespace JuniorCodeCRM.Helpers;

public static class ValidationHelper
{
    /// <summary>
    /// Проверка ФИО: кириллица, латиница, пробелы, дефисы, 2-50 символов
    /// </summary>
    public static bool IsValidName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (name.Length < 2 || name.Length > 50) return false;
        return Regex.IsMatch(name, @"^[а-яА-ЯёЁa-zA-Z\- ]+$");
    }

    /// <summary>
    /// Проверка телефона: маска +7 (XXX) XXX-XX-XX или 8 (XXX) XXX-XX-XX
    /// </summary>
    public static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return true; // Телефон необязателен
        return Regex.IsMatch(phone, @"^(\+7|8)\s*\(\d{3}\)\s*\d{3}[-\s]?\d{2}[-\s]?\d{2}$");
    }

    /// <summary>
    /// Проверка email
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return true; // Email необязателен
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    /// <summary>
    /// Проверка: дедлайн не раньше даты создания
    /// </summary>
    public static bool IsValidDeadline(DateTime? deadline, DateTime createdDate)
    {
        if (!deadline.HasValue) return true; // Дедлайн необязателен
        return deadline.Value.Date >= createdDate.Date;
    }

    /// <summary>
    /// Очистка строки от лишних пробелов
    /// </summary>
    public static string TrimExcess(string? input, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var trimmed = Regex.Replace(input.Trim(), @"\s+", " ");
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }
}