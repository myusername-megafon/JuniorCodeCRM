namespace JuniorCodeCRM.Models.DTOs.Employee;

public class EmployeeFilterDto
{
    public string? Search { get; set; }          // Поиск по ФИО
    public int? DepartmentID { get; set; }       // Фильтр по отделу
    public int? PositionID { get; set; }         // Фильтр по должности
    public bool? IsActive { get; set; }          // Активные/архивные
    public bool? IsCombined { get; set; }        // Только совместители

    // Пагинация
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}