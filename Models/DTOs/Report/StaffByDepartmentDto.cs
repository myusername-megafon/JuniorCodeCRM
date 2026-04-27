namespace JuniorCodeCRM.Models.DTOs.Report;

public class StaffByDepartmentDto
{
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int CombinedCount { get; set; }
}