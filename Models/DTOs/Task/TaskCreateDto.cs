using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.DTOs.Task;

public class TaskCreateDto
{
    [Required(ErrorMessage = "Название поручения обязательно")]
    [MinLength(3, ErrorMessage = "Название должно содержать минимум 3 символа")]
    [MaxLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Исполнитель обязателен")]
    public int AssigneeID { get; set; }

    public int StatusID { get; set; } = 1;       // По умолчанию "В работе"

    [Required(ErrorMessage = "Приоритет обязателен")]
    public int PriorityID { get; set; }

    public DateTime? Deadline { get; set; }

    [MaxLength(50)]
    public string? BoardColumn { get; set; }

    public int SortOrder { get; set; } = 0;
}