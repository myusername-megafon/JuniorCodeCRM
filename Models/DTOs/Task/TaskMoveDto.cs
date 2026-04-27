using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.DTOs.Task;

/// <summary>
/// DTO для перемещения поручения между колонками доски (Trello-like drag & drop)
/// </summary>
public class TaskMoveDto
{
    [Required(ErrorMessage = "ID поручения обязателен")]
    public int TaskID { get; set; }

    /// <summary>
    /// Новая колонка ("В работе", "На проверке", "Готово", и т.д.)
    /// </summary>
    [Required(ErrorMessage = "Колонка обязательна")]
    [MaxLength(50)]
    public string NewColumn { get; set; } = string.Empty;

    /// <summary>
    /// Новый порядковый номер в колонке
    /// </summary>
    public int NewSortOrder { get; set; }

    /// <summary>
    /// Новый статус (если перемещение меняет статус)
    /// </summary>
    public int? NewStatusID { get; set; }
}