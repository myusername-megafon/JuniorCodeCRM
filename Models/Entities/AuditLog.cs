using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.Entities;

public class AuditLog
{
    [Key]
    public int LogID { get; set; }

    [Required, MaxLength(50)]
    public string ActionType { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    public int? EntityID { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.Now;

    [MaxLength(45)]
    public string? IPAddress { get; set; }
}