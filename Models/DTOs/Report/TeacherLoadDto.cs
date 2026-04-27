namespace JuniorCodeCRM.Models.DTOs.Report;

public class TeacherLoadDto
{
    public string TeacherName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int TotalClasses { get; set; }
    public int TotalMinutes { get; set; }
    public decimal TotalHours { get; set; }
    public int RecurringClasses { get; set; }
}