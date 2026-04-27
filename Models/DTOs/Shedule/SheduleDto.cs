namespace JuniorCodeCRM.Models.DTOs.Schedule;

public class ScheduleDto
{
    public int ScheduleID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Direction { get; set; }

    public int TeacherID { get; set; }
    public string TeacherName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int Duration { get; set; }

    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }
    public byte? DayOfWeek { get; set; }

    // День недели текстом
    public string? DayOfWeekName => DayOfWeek switch
    {
        1 => "Понедельник",
        2 => "Вторник",
        3 => "Среда",
        4 => "Четверг",
        5 => "Пятница",
        6 => "Суббота",
        7 => "Воскресенье",
        _ => null
    };

    public string? Room { get; set; }
    public int? MaxStudents { get; set; }
    public bool IsActive { get; set; }

    public TimeSpan EndTime => StartTime.Add(TimeSpan.FromMinutes(Duration));
}