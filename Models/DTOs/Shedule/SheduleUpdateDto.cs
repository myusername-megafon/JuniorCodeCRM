using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.DTOs.Schedule;

public class ScheduleUpdateDto
{
    [Required(ErrorMessage = "Название занятия обязательно")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Direction { get; set; }

    [Required(ErrorMessage = "Преподаватель обязателен")]
    public int TeacherID { get; set; }

    [Required(ErrorMessage = "Дата начала обязательна")]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required(ErrorMessage = "Время начала обязательно")]
    public TimeSpan StartTime { get; set; }

    [Range(15, 480, ErrorMessage = "Длительность должна быть от 15 до 480 минут")]
    public int Duration { get; set; } = 60;

    public bool IsRecurring { get; set; }

    [MaxLength(100)]
    public string? RecurrenceRule { get; set; }

    [Range(1, 7)]
    public byte? DayOfWeek { get; set; }

    [MaxLength(50)]
    public string? Room { get; set; }

    [Range(1, 100)]
    public int? MaxStudents { get; set; }

    public bool IsActive { get; set; } = true;
}