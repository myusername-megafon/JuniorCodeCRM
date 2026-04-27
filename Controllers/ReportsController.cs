using JuniorCodeCRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JuniorCodeCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Отчёт «Кадровый состав по отделам»
    /// </summary>
    [HttpGet("staff-by-department")]
    public async Task<IActionResult> GetStaffByDepartment()
    {
        var report = await _reportService.GetStaffByDepartmentReportAsync();
        return Ok(report);
    }

    /// <summary>
    /// Отчёт «Исполнение поручений»
    /// </summary>
    [HttpGet("task-execution")]
    public async Task<IActionResult> GetTaskExecution()
    {
        var report = await _reportService.GetTaskExecutionReportAsync();
        return Ok(report);
    }

    /// <summary>
    /// Отчёт «Загрузка преподавателей»
    /// </summary>
    [HttpGet("teacher-load")]
    public async Task<IActionResult> GetTeacherLoad()
    {
        var report = await _reportService.GetTeacherLoadReportAsync();
        return Ok(report);
    }
}