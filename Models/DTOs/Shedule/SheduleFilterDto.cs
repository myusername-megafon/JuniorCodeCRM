namespace JuniorCodeCRM.Models.DTOs.Schedule;

public class ScheduleFilterDto
{
    public string? Search { get; set; }
    public string? Direction { get; set; }
    public int? TeacherID { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? IsRecurring { get; set; }
    public bool? IsActive { get; set; }

    // Пагинация
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}