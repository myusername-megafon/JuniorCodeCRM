using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.Entities;

public class TaskStatus
{
    [Key]
    public int StatusID { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(7)]
    public string? ColorHex { get; set; }
}