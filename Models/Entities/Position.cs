using System.ComponentModel.DataAnnotations;

namespace JuniorCodeCRM.Models.Entities;

public class Position
{
    [Key]
    public int PositionID { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool CanBeCombined { get; set; }
}