using JuniorCodeCRM.Data;
using JuniorCodeCRM.Helpers;
using JuniorCodeCRM.Models.DTOs.Task;
using JuniorCodeCRM.Models.Entities;
using JuniorCodeCRM.Models.Enums;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JuniorCodeCRM.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public TaskService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<TaskDto>> GetAllAsync(TaskFilterDto filter)
    {
        var query = _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .AsQueryable();

        // Фильтрация
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(search) ||
                                     (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        if (filter.StatusID.HasValue)
            query = query.Where(t => t.StatusID == filter.StatusID.Value);

        if (filter.PriorityID.HasValue)
            query = query.Where(t => t.PriorityID == filter.PriorityID.Value);

        if (filter.AssigneeID.HasValue)
            query = query.Where(t => t.AssigneeID == filter.AssigneeID.Value);

        if (!string.IsNullOrWhiteSpace(filter.BoardColumn))
            query = query.Where(t => t.BoardColumn == filter.BoardColumn);

        if (filter.DeadlineFrom.HasValue)
            query = query.Where(t => t.Deadline >= filter.DeadlineFrom.Value);

        if (filter.DeadlineTo.HasValue)
            query = query.Where(t => t.Deadline <= filter.DeadlineTo.Value);

        // Сортировка: сначала по колонке, потом по порядку, потом по дедлайну
        query = query
            .OrderBy(t => t.BoardColumn)
            .ThenBy(t => t.SortOrder)
            .ThenBy(t => t.Deadline)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize);

        return await query.Select(t => MapToDto(t)).ToListAsync();
    }

    public async Task<TaskDto?> GetByIdAsync(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .FirstOrDefaultAsync(t => t.TaskID == id);

        return task == null ? null : MapToDto(task);
    }

    public async Task<TaskDto> CreateAsync(TaskCreateDto dto, string? ipAddress)
    {
        // Валидация
        ValidateTask(dto.Title, dto.Description, dto.Deadline);

        var task = new TaskItem
        {
            Title = ValidationHelper.TrimExcess(dto.Title, 100),
            Description = dto.Description != null ? ValidationHelper.TrimExcess(dto.Description, 500) : null,
            AssigneeID = dto.AssigneeID,
            StatusID = dto.StatusID,
            PriorityID = dto.PriorityID,
            CreatedDate = DateTime.Now,
            Deadline = dto.Deadline,
            BoardColumn = dto.BoardColumn,
            SortOrder = dto.SortOrder
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var createdTask = await GetByIdAsync(task.TaskID);

        await _auditService.LogAsync(ActionType.CREATE, EntityType.Task, task.TaskID,
            null, JsonHelper.Serialize(createdTask), ipAddress);

        return createdTask!;
    }

    public async Task<TaskDto?> UpdateAsync(int id, TaskUpdateDto dto, string? ipAddress)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .FirstOrDefaultAsync(t => t.TaskID == id);

        if (task == null) return null;

        var oldValues = JsonHelper.Serialize(MapToDto(task));

        ValidateTask(dto.Title, dto.Description, dto.Deadline);

        task.Title = ValidationHelper.TrimExcess(dto.Title, 100);
        task.Description = dto.Description != null ? ValidationHelper.TrimExcess(dto.Description, 500) : null;
        task.AssigneeID = dto.AssigneeID;
        task.StatusID = dto.StatusID;
        task.PriorityID = dto.PriorityID;
        task.Deadline = dto.Deadline;
        task.BoardColumn = dto.BoardColumn;
        task.SortOrder = dto.SortOrder;

        // Если статус "Выполнено", ставим дату завершения
        if (dto.StatusID == 2) // ID статуса "Выполнено"
        {
            task.CompletedDate = dto.CompletedDate ?? DateTime.Now;
        }
        else
        {
            task.CompletedDate = dto.CompletedDate;
        }

        await _context.SaveChangesAsync();

        var newValues = JsonHelper.Serialize(MapToDto(task));

        await _auditService.LogAsync(ActionType.UPDATE, EntityType.Task, task.TaskID,
            oldValues, newValues, ipAddress);

        return MapToDto(task);
    }

    public async Task<TaskDto?> MoveAsync(TaskMoveDto dto, string? ipAddress)
    {
        var task = await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Status)
            .Include(t => t.Priority)
            .FirstOrDefaultAsync(t => t.TaskID == dto.TaskID);

        if (task == null) return null;

        var oldValues = JsonHelper.Serialize(MapToDto(task));

        // Перемещение между колонками
        task.BoardColumn = dto.NewColumn;
        task.SortOrder = dto.NewSortOrder;

        // Если передан новый статус, обновляем
        if (dto.NewStatusID.HasValue)
        {
            task.StatusID = dto.NewStatusID.Value;

            // Если переместили в "Выполнено", ставим дату завершения
            if (dto.NewStatusID.Value == 2) // "Выполнено"
            {
                task.CompletedDate = DateTime.Now;
            }
        }

        await _context.SaveChangesAsync();

        var newValues = JsonHelper.Serialize(MapToDto(task));

        await _auditService.LogAsync(ActionType.UPDATE, EntityType.Task, task.TaskID,
            oldValues, newValues, ipAddress);

        return MapToDto(task);
    }

    public async Task<bool> DeleteAsync(int id, string? ipAddress)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        var oldValues = JsonHelper.Serialize(MapToDto(task));

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(ActionType.DELETE, EntityType.Task, id,
            oldValues, null, ipAddress);

        return true;
    }

    public async Task<int> GetTotalCountAsync(TaskFilterDto filter)
    {
        var query = _context.Tasks.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(search));
        }

        if (filter.StatusID.HasValue)
            query = query.Where(t => t.StatusID == filter.StatusID.Value);

        if (filter.AssigneeID.HasValue)
            query = query.Where(t => t.AssigneeID == filter.AssigneeID.Value);

        return await query.CountAsync();
    }

    private static TaskDto MapToDto(TaskItem t)
    {
        return new TaskDto
        {
            TaskID = t.TaskID,
            Title = t.Title,
            Description = t.Description,
            AssigneeID = t.AssigneeID,
            AssigneeName = t.Assignee != null
                ? $"{t.Assignee.LastName} {t.Assignee.FirstName}".Trim()
                : string.Empty,
            StatusID = t.StatusID,
            StatusName = t.Status?.Name ?? string.Empty,
            StatusColor = t.Status?.ColorHex,
            PriorityID = t.PriorityID,
            PriorityName = t.Priority?.Name ?? string.Empty,
            PriorityColor = t.Priority?.ColorHex,
            CreatedDate = t.CreatedDate,
            Deadline = t.Deadline,
            CompletedDate = t.CompletedDate,
            BoardColumn = t.BoardColumn,
            SortOrder = t.SortOrder
        };
    }

    private static void ValidateTask(string title, string? description, DateTime? deadline)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(title) || title.Length < 3 || title.Length > 100)
            errors.Add("Название поручения должно быть от 3 до 100 символов");

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
            errors.Add("Описание не должно превышать 500 символов");

        if (deadline.HasValue && deadline.Value.Date < DateTime.Now.Date)
            errors.Add("Дедлайн не может быть раньше текущей даты");

        if (errors.Any())
            throw new ArgumentException(string.Join("; ", errors));
    }
}