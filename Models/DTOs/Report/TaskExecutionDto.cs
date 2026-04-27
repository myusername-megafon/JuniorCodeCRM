namespace JuniorCodeCRM.Models.DTOs.Report;

public class TaskExecutionDto
{
    public string Status { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;
    public int TaskCount { get; set; }
    public int OverdueCount { get; set; }
    public decimal CompletionPercent { get; set; }
}