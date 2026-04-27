using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.Entities;

public class ScheduleItem
{
    [Key]
    public int ScheduleID { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Direction { get; set; }

    public int TeacherID { get; set; }
    public Employee Teacher { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int Duration { get; set; } = 60;
    public bool IsRecurring { get; set; }

    [MaxLength(100)]
    public string? RecurrenceRule { get; set; }

    public byte? DayOfWeek { get; set; }

    [MaxLength(50)]
    public string? Room { get; set; }

    public int? MaxStudents { get; set; }
    public bool IsActive { get; set; } = true;
}