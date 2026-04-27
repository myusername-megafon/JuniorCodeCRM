using JuniorCodeCRM.Models.DTOs.Schedule;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JuniorCodeCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    /// <summary>
    /// Получить расписание с фильтрацией и пагинацией
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ScheduleFilterDto filter)
    {
        var schedules = await _scheduleService.GetAllAsync(filter);
        var totalCount = await _scheduleService.GetTotalCountAsync(filter);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        return Ok(schedules);
    }

    /// <summary>
    /// Получить занятие по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var schedule = await _scheduleService.GetByIdAsync(id);
        if (schedule == null)
            return NotFound(new { error = "Занятие не найдено" });

        return Ok(schedule);
    }

    /// <summary>
    /// Создать новое занятие
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ScheduleCreateDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var schedule = await _scheduleService.CreateAsync(dto, ip);

        return CreatedAtAction(nameof(GetById), new { id = schedule.ScheduleID }, schedule);
    }

    /// <summary>
    /// Обновить занятие
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ScheduleUpdateDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var schedule = await _scheduleService.UpdateAsync(id, dto, ip);

        if (schedule == null)
            return NotFound(new { error = "Занятие не найдено" });

        return Ok(schedule);
    }

    /// <summary>
    /// Удалить занятие
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _scheduleService.DeleteAsync(id, ip);

        if (!result)
            return NotFound(new { error = "Занятие не найдено" });

        return Ok(new { message = "Занятие удалено" });
    }
}