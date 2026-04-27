using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.Entities;

public class TaskItem
{
    [Key]
    public int TaskID { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int AssigneeID { get; set; }
    public Employee Assignee { get; set; } = null!;

    public int StatusID { get; set; }
    public TaskStatus Status { get; set; } = null!;

    public int PriorityID { get; set; }
    public TaskPriority Priority { get; set; } = null!;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? Deadline { get; set; }
    public DateTime? CompletedDate { get; set; }

    [MaxLength(50)]
    public string? BoardColumn { get; set; }

    public int SortOrder { get; set; }
}