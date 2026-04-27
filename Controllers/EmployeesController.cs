using JuniorCodeCRM.Models.DTOs.Employee;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JuniorCodeCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Получить список сотрудников с фильтрацией и пагинацией
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeFilterDto filter)
    {
        var employees = await _employeeService.GetAllAsync(filter);
        var totalCount = await _employeeService.GetTotalCountAsync(filter);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        return Ok(employees);
    }

    /// <summary>
    /// Получить сотрудника по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
            return NotFound(new { error = "Сотрудник не найден" });

        return Ok(employee);
    }

    /// <summary>
    /// Создать нового сотрудника
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var employee = await _employeeService.CreateAsync(dto, ip);

        return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeID }, employee);
    }

    /// <summary>
    /// Обновить данные сотрудника
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var employee = await _employeeService.UpdateAsync(id, dto, ip);

        if (employee == null)
            return NotFound(new { error = "Сотрудник не найден" });

        return Ok(employee);
    }

    /// <summary>
    /// Архивировать сотрудника (мягкое удаление)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _employeeService.DeleteAsync(id, ip);

        if (!result)
            return NotFound(new { error = "Сотрудник не найден" });

        return Ok(new { message = "Сотрудник архивирован" });
    }
}