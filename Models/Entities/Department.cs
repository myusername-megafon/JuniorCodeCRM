using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.Entities;

public class Department
{
    [Key]
    public int DepartmentID { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}