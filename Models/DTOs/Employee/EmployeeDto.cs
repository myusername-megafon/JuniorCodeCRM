namespace JuniorCodeCRM.Models.DTOs.Employee;

public class EmployeeDto
{
    public int EmployeeID { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

    public int PositionID { get; set; }
    public string PositionName { get; set; } = string.Empty;

    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsCombined { get; set; }
    public int? CombinedPositionID { get; set; }
    public string? CombinedPositionName { get; set; }
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}