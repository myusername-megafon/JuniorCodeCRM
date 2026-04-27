using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JuniorCodeCRM.Models.Entities;

public class Employee
{
    [Key]
    public int EmployeeID { get; set; }

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MiddleName { get; set; }

    public int PositionID { get; set; }
    public Position Position { get; set; } = null!;

    public int DepartmentID { get; set; }
    public Department Department { get; set; } = null!;

    [MaxLength(18)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    public bool IsCombined { get; set; }

    public int? CombinedPositionID { get; set; }
    public Position? CombinedPosition { get; set; }

    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Навигационные свойства
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
}