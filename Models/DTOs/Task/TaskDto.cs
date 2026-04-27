namespace JuniorCodeCRM.Models.DTOs.Task;

public class TaskDto
{
    public int TaskID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int AssigneeID { get; set; }
    public string AssigneeName { get; set; } = string.Empty;

    public int StatusID { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? StatusColor { get; set; }

    public int PriorityID { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public string? PriorityColor { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? CompletedDate { get; set; }

    public string? BoardColumn { get; set; }
    public int SortOrder { get; set; }

    // Вычисляемые поля
    public bool IsOverdue => Deadline.HasValue
        && Deadline.Value.Date < DateTime.Now.Date
        && StatusID == 1; // "В работе"
}