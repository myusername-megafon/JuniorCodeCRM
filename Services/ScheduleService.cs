using JuniorCodeCRM.Data;
using JuniorCodeCRM.Helpers;
using JuniorCodeCRM.Models.DTOs.Schedule;
using JuniorCodeCRM.Models.Entities;
using JuniorCodeCRM.Models.Enums;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JuniorCodeCRM.Services;

public class ScheduleService : IScheduleService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public ScheduleService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<ScheduleDto>> GetAllAsync(ScheduleFilterDto filter)
    {
        var query = _context.Schedule
            .Include(s => s.Teacher)
            .AsQueryable();

        // Фильтрация
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLower();
            query = query.Where(s => s.Title.ToLower().Contains(search) ||
                                     (s.Direction != null && s.Direction.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Direction))
            query = query.Where(s => s.Direction == filter.Direction);

        if (filter.TeacherID.HasValue)
            query = query.Where(s => s.TeacherID == filter.TeacherID.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(s => s.StartDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(s => s.StartDate <= filter.DateTo.Value);

        if (filter.IsRecurring.HasValue)
            query = query.Where(s => s.IsRecurring == filter.IsRecurring.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filter.IsActive.Value);

        // Сортировка: сначала по дате начала, потом по времени
        query = query
            .OrderBy(s => s.StartDate)
            .ThenBy(s => s.StartTime)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize);

        return await query.Select(s => MapToDto(s)).ToListAsync();
    }

    public async Task<ScheduleDto?> GetByIdAsync(int id)
    {
        var schedule = await _context.Schedule
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.ScheduleID == id);

        return schedule == null ? null : MapToDto(schedule);
    }

    public async Task<ScheduleDto> CreateAsync(ScheduleCreateDto dto, string? ipAddress)
    {
        ValidateSchedule(dto);

        var schedule = new ScheduleItem
        {
            Title = ValidationHelper.TrimExcess(dto.Title, 200),
            Direction = dto.Direction != null ? ValidationHelper.TrimExcess(dto.Direction, 100) : null,
            TeacherID = dto.TeacherID,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate?.Date,
            StartTime = dto.StartTime,
            Duration = dto.Duration,
            IsRecurring = dto.IsRecurring,
            RecurrenceRule = dto.IsRecurring ? dto.RecurrenceRule : null,
            DayOfWeek = dto.IsRecurring ? dto.DayOfWeek : null,
            Room = dto.Room,
            MaxStudents = dto.MaxStudents
        };

        _context.Schedule.Add(schedule);
        await _context.SaveChangesAsync();

        var createdSchedule = await GetByIdAsync(schedule.ScheduleID);

        await _auditService.LogAsync(ActionType.CREATE, EntityType.Schedule, schedule.ScheduleID,
            null, JsonHelper.Serialize(createdSchedule), ipAddress);

        return createdSchedule!;
    }

    public async Task<ScheduleDto?> UpdateAsync(int id, ScheduleUpdateDto dto, string? ipAddress)
    {
        var schedule = await _context.Schedule
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.ScheduleID == id);

        if (schedule == null) return null;

        var oldValues = JsonHelper.Serialize(MapToDto(schedule));

        ValidateSchedule(dto);

        schedule.Title = ValidationHelper.TrimExcess(dto.Title, 200);
        schedule.Direction = dto.Direction != null ? ValidationHelper.TrimExcess(dto.Direction, 100) : null;
        schedule.TeacherID = dto.TeacherID;
        schedule.StartDate = dto.StartDate.Date;
        schedule.EndDate = dto.EndDate?.Date;
        schedule.StartTime = dto.StartTime;
        schedule.Duration = dto.Duration;
        schedule.IsRecurring = dto.IsRecurring;
        schedule.RecurrenceRule = dto.IsRecurring ? dto.RecurrenceRule : null;
        schedule.DayOfWeek = dto.IsRecurring ? dto.DayOfWeek : null;
        schedule.Room = dto.Room;
        schedule.MaxStudents = dto.MaxStudents;
        schedule.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        var newValues = JsonHelper.Serialize(MapToDto(schedule));

        await _auditService.LogAsync(ActionType.UPDATE, EntityType.Schedule, schedule.ScheduleID,
            oldValues, newValues, ipAddress);

        return MapToDto(schedule);
    }

    public async Task<bool> DeleteAsync(int id, string? ipAddress)
    {
        var schedule = await _context.Schedule.FindAsync(id);
        if (schedule == null) return false;

        var oldValues = JsonHelper.Serialize(MapToDto(schedule));

        _context.Schedule.Remove(schedule);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(ActionType.DELETE, EntityType.Schedule, id,
            oldValues, null, ipAddress);

        return true;
    }

    public async Task<int> GetTotalCountAsync(ScheduleFilterDto filter)
    {
        var query = _context.Schedule.AsQueryable();

        if (filter.TeacherID.HasValue)
            query = query.Where(s => s.TeacherID == filter.TeacherID.Value);

        if (filter.IsActive.HasValue)
            query = query.Where(s => s.IsActive == filter.IsActive.Value);

        return await query.CountAsync();
    }

    private static ScheduleDto MapToDto(ScheduleItem s)
    {
        return new ScheduleDto
        {
            ScheduleID = s.ScheduleID,
            Title = s.Title,
            Direction = s.Direction,
            TeacherID = s.TeacherID,
            TeacherName = s.Teacher != null
                ? $"{s.Teacher.LastName} {s.Teacher.FirstName}".Trim()
                : string.Empty,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            StartTime = s.StartTime,
            Duration = s.Duration,
            IsRecurring = s.IsRecurring,
            RecurrenceRule = s.RecurrenceRule,
            DayOfWeek = s.DayOfWeek,
            Room = s.Room,
            MaxStudents = s.MaxStudents,
            IsActive = s.IsActive
        };
    }

    private static void ValidateSchedule(ScheduleCreateDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length > 200)
            errors.Add("Название занятия обязательно и не должно превышать 200 символов");

        if (dto.Duration < 15 || dto.Duration > 480)
            errors.Add("Длительность должна быть от 15 до 480 минут");

        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
            errors.Add("Дата окончания не может быть раньше даты начала");

        if (dto.IsRecurring && !dto.DayOfWeek.HasValue)
            errors.Add("Для циклических занятий необходимо указать день недели");

        if (dto.MaxStudents.HasValue && (dto.MaxStudents.Value < 1 || dto.MaxStudents.Value > 100))
            errors.Add("Количество учеников должно быть от 1 до 100");

        if (errors.Any())
            throw new ArgumentException(string.Join("; ", errors));
    }

    // Перегрузка для UpdateDto
    private static void ValidateSchedule(ScheduleUpdateDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Title) || dto.Title.Length > 200)
            errors.Add("Название занятия обязательно и не должно превышать 200 символов");

        if (dto.Duration < 15 || dto.Duration > 480)
            errors.Add("Длительность должна быть от 15 до 480 минут");

        if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
            errors.Add("Дата окончания не может быть раньше даты начала");

        if (dto.IsRecurring && !dto.DayOfWeek.HasValue)
            errors.Add("Для циклических занятий необходимо указать день недели");

        if (dto.MaxStudents.HasValue && (dto.MaxStudents.Value < 1 || dto.MaxStudents.Value > 100))
            errors.Add("Количество учеников должно быть от 1 до 100");

        if (errors.Any())
            throw new ArgumentException(string.Join("; ", errors));
    }
}