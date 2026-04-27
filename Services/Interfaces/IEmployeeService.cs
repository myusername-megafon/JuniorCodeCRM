using JuniorCodeCRM.Models.DTOs.Employee;

namespace JuniorCodeCRM.Services.Interfaces;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync(EmployeeFilterDto filter);
    Task<EmployeeDto?> GetByIdAsync(int id);
    Task<EmployeeDto> CreateAsync(EmployeeCreateDto dto, string? ipAddress);
    Task<EmployeeDto?> UpdateAsync(int id, EmployeeUpdateDto dto, string? ipAddress);
    Task<bool> DeleteAsync(int id, string? ipAddress);
    Task<int> GetTotalCountAsync(EmployeeFilterDto filter);
}