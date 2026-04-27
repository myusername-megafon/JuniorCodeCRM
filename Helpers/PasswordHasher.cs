namespace JuniorCodeCRM.Helpers;

public static class PasswordHasher
{
    /// <summary>
    /// Хеширует пароль с использованием BCrypt
    /// </summary>
    public static string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    /// <summary>
    /// Проверяет соответствие пароля хешу
    /// </summary>
    public static bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // Если хеш битый — возвращаем false
            return false;
        }
    }
}