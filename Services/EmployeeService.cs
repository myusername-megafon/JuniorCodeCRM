using JuniorCodeCRM.Data;
using JuniorCodeCRM.Helpers;
using JuniorCodeCRM.Models.DTOs.Employee;
using JuniorCodeCRM.Models.Entities;
using JuniorCodeCRM.Models.Enums;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JuniorCodeCRM.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public EmployeeService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<EmployeeDto>> GetAllAsync(EmployeeFilterDto filter)
    {
        var query = _context.Employees
            .Include(e => e.Position)
            .Include(e => e.CombinedPosition)
            .Include(e => e.Department)
            .AsQueryable();

        // Фильтрация
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(e =>
                e.LastName.ToLower().Contains(search) ||
                e.FirstName.ToLower().Contains(search) ||
                (e.MiddleName != null && e.MiddleName.ToLower().Contains(search)));
        }

        if (filter.DepartmentID.HasValue)
            query = query.Where(e => e.DepartmentID == filter.DepartmentID.Value);

        if (filter.PositionID.HasValue)
            query = query.Where(e => e.PositionID == filter.PositionID.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(e => e.IsActive == filter.IsActive.Value);

        if (filter.IsCombined.HasValue)
            query = query.Where(e => e.IsCombined == filter.IsCombined.Value);

        // Пагинация
        query = query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize);

        return await query.Select(e => MapToDto(e)).ToListAsync();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Position)
            .Include(e => e.CombinedPosition)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeID == id);

        return employee == null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(EmployeeCreateDto dto, string? ipAddress)
    {
        // Валидация
        ValidateEmployee(dto.LastName, dto.FirstName, dto.MiddleName, dto.Phone, dto.Email);

        var employee = new Employee
        {
            LastName = ValidationHelper.TrimExcess(dto.LastName, 50),
            FirstName = ValidationHelper.TrimExcess(dto.FirstName, 50),
            MiddleName = dto.MiddleName != null ? ValidationHelper.TrimExcess(dto.MiddleName, 50) : null,
            PositionID = dto.PositionID,
            DepartmentID = dto.DepartmentID,
            Phone = dto.Phone,
            Email = dto.Email,
            IsCombined = dto.IsCombined,
            CombinedPositionID = dto.IsCombined ? dto.CombinedPositionID : null,
            HireDate = dto.HireDate ?? DateTime.Now,
            Notes = dto.Notes != null ? ValidationHelper.TrimExcess(dto.Notes, 500) : null
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(ActionType.CREATE, EntityType.Employee, employee.EmployeeID,
            null, JsonHelper.Serialize(MapToDto(employee)), ipAddress);

        return MapToDto(employee);
    }

    public async Task<EmployeeDto?> UpdateAsync(int id, EmployeeUpdateDto dto, string? ipAddress)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return null;

        var oldValues = JsonHelper.Serialize(MapToDto(employee));

        ValidateEmployee(dto.LastName, dto.FirstName, dto.MiddleName, dto.Phone, dto.Email);

        employee.LastName = ValidationHelper.TrimExcess(dto.LastName, 50);
        employee.FirstName = ValidationHelper.TrimExcess(dto.FirstName, 50);
        employee.MiddleName = dto.MiddleName != null ? ValidationHelper.TrimExcess(dto.MiddleName, 50) : null;
        employee.PositionID = dto.PositionID;
        employee.DepartmentID = dto.DepartmentID;
        employee.Phone = dto.Phone;
        employee.Email = dto.Email;
        employee.IsCombined = dto.IsCombined;
        employee.CombinedPositionID = dto.IsCombined ? dto.CombinedPositionID : null;
        employee.HireDate = dto.HireDate;
        employee.IsActive = dto.IsActive;
        employee.Notes = dto.Notes != null ? ValidationHelper.TrimExcess(dto.Notes, 500) : null;

        await _context.SaveChangesAsync();

        var newValues = JsonHelper.Serialize(MapToDto(employee));

        await _auditService.LogAsync(ActionType.UPDATE, EntityType.Employee, employee.EmployeeID,
            oldValues, newValues, ipAddress);

        return MapToDto(employee);
    }

    public async Task<bool> DeleteAsync(int id, string? ipAddress)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        // Мягкое удаление (архивация)
        var oldValues = JsonHelper.Serialize(MapToDto(employee));
        employee.IsActive = false;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(ActionType.DELETE, EntityType.Employee, employee.EmployeeID,
            oldValues, JsonHelper.Serialize(MapToDto(employee)), ipAddress);

        return true;
    }

    public async Task<int> GetTotalCountAsync(EmployeeFilterDto filter)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(e =>
                e.LastName.ToLower().Contains(search) ||
                e.FirstName.ToLower().Contains(search));
        }

        if (filter.DepartmentID.HasValue)
            query = query.Where(e => e.DepartmentID == filter.DepartmentID.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(e => e.IsActive == filter.IsActive.Value);

        return await query.CountAsync();
    }

    private static EmployeeDto MapToDto(Employee e)
    {
        return new EmployeeDto
        {
            EmployeeID = e.EmployeeID,
            LastName = e.LastName,
            FirstName = e.FirstName,
            MiddleName = e.MiddleName,
            PositionID = e.PositionID,
            PositionName = e.Position?.Name ?? string.Empty,
            DepartmentID = e.DepartmentID,
            DepartmentName = e.Department?.Name ?? string.Empty,
            Phone = e.Phone,
            Email = e.Email,
            IsCombined = e.IsCombined,
            CombinedPositionID = e.CombinedPositionID,
            CombinedPositionName = e.CombinedPosition?.Name,
            HireDate = e.HireDate,
            IsActive = e.IsActive,
            Notes = e.Notes
        };
    }

    private static void ValidateEmployee(string lastName, string firstName, string? middleName,
        string? phone, string? email)
    {
        var errors = new List<string>();

        if (!ValidationHelper.IsValidName(lastName))
            errors.Add("Фамилия содержит недопустимые символы или неверную длину");

        if (!ValidationHelper.IsValidName(firstName))
            errors.Add("Имя содержит недопустимые символы или неверную длину");

        if (!string.IsNullOrWhiteSpace(middleName) && !ValidationHelper.IsValidName(middleName))
            errors.Add("Отчество содержит недопустимые символы или неверную длину");

        if (!string.IsNullOrWhiteSpace(phone) && !ValidationHelper.IsValidPhone(phone))
            errors.Add("Телефон не соответствует маске +7 (XXX) XXX-XX-XX");

        if (!string.IsNullOrWhiteSpace(email) && !ValidationHelper.IsValidEmail(email))
            errors.Add("Email имеет неверный формат");

        if (errors.Any())
            throw new ArgumentException(string.Join("; ", errors));
    }
}