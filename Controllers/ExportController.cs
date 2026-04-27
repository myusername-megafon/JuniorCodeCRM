using JuniorCodeCRM.Helpers;
using JuniorCodeCRM.Models.Enums;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JuniorCodeCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// Экспорт отчёта «Кадровый состав по отделам»
    /// </summary>
    [HttpGet("staff-by-department")]
    public async Task<IActionResult> ExportStaffByDepartment([FromQuery] ExportFormat format = ExportFormat.XLSX)
    {
        var fileBytes = await _exportService.ExportStaffByDepartmentAsync(format);
        var fileName = ExportHelper.GetFileName("Кадровый_состав", format.ToString());
        var contentType = ExportHelper.GetContentType(format.ToString());

        return File(fileBytes, contentType, fileName);
    }

    /// <summary>
    /// Экспорт отчёта «Исполнение поручений»
    /// </summary>
    [HttpGet("task-execution")]
    public async Task<IActionResult> ExportTaskExecution([FromQuery] ExportFormat format = ExportFormat.XLSX)
    {
        var fileBytes = await _exportService.ExportTaskExecutionAsync(format);
        var fileName = ExportHelper.GetFileName("Исполнение_поручений", format.ToString());
        var contentType = ExportHelper.GetContentType(format.ToString());

        return File(fileBytes, contentType, fileName);
    }

    /// <summary>
    /// Экспорт отчёта «Загрузка преподавателей»
    /// </summary>
    [HttpGet("teacher-load")]
    public async Task<IActionResult> ExportTeacherLoad([FromQuery] ExportFormat format = ExportFormat.XLSX)
    {
        var fileBytes = await _exportService.ExportTeacherLoadAsync(format);
        var fileName = ExportHelper.GetFileName("Загрузка_преподавателей", format.ToString());
        var contentType = ExportHelper.GetContentType(format.ToString());

        return File(fileBytes, contentType, fileName);
    }
}