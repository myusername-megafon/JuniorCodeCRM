using JuniorCodeCRM.Models.DTOs.Task;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JuniorCodeCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Получить список поручений с фильтрацией
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TaskFilterDto filter)
    {
        var tasks = await _taskService.GetAllAsync(filter);
        var totalCount = await _taskService.GetTotalCountAsync(filter);

        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        return Ok(tasks);
    }

    /// <summary>
    /// Получить поручение по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
            return NotFound(new { error = "Поручение не найдено" });

        return Ok(task);
    }

    /// <summary>
    /// Создать новое поручение
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskCreateDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var task = await _taskService.CreateAsync(dto, ip);

        return CreatedAtAction(nameof(GetById), new { id = task.TaskID }, task);
    }

    /// <summary>
    /// Обновить поручение
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskUpdateDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var task = await _taskService.UpdateAsync(id, dto, ip);

        if (task == null)
            return NotFound(new { error = "Поручение не найдено" });

        return Ok(task);
    }

    /// <summary>
    /// Переместить поручение между колонками (Trello-like drag & drop)
    /// </summary>
    [HttpPatch("move")]
    public async Task<IActionResult> Move([FromBody] TaskMoveDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var task = await _taskService.MoveAsync(dto, ip);

        if (task == null)
            return NotFound(new { error = "Поручение не найдено" });

        return Ok(task);
    }

    /// <summary>
    /// Удалить поручение
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _taskService.DeleteAsync(id, ip);

        if (!result)
            return NotFound(new { error = "Поручение не найдено" });

        return Ok(new { message = "Поручение удалено" });
    }
}