using JuniorCodeCRM.Data;
using JuniorCodeCRM.Models.DTOs.Dashboard;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JuniorCodeCRM.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var today = DateTime.Now.Date;
        var currentDayOfWeek = (int)DateTime.Now.DayOfWeek;
        if (currentDayOfWeek == 0) currentDayOfWeek = 7; // Воскресенье = 7 (SQL Sunday = 1)

        // Базовые метрики
        var totalEmployees = await _context.Employees.CountAsync(e => e.IsActive);
        var activeSchedules = await _context.Schedule.CountAsync(s =>
            s.IsActive &&
            s.StartDate <= today &&
            (s.EndDate == null || s.EndDate >= today));

        var todayClasses = await _context.Schedule.CountAsync(s =>
            s.IsActive &&
            s.StartDate <= today &&
            (s.EndDate == null || s.EndDate >= today) &&
            (!s.IsRecurring && s.StartDate == today ||
             s.IsRecurring && s.StartDate <= today && s.DayOfWeek == currentDayOfWeek));

        var tasksInProgress = await _context.Tasks.CountAsync(t => t.StatusID == 1);
        var tasksOverdue = await _context.Tasks.CountAsync(t =>
            t.Deadline != null &&
            t.Deadline.Value.Date < today &&
            (t.StatusID == 1 || t.StatusID == 4));

        var tasksNearDeadline = await _context.Tasks.CountAsync(t =>
            t.Deadline != null &&
            t.Deadline.Value.Date >= today &&
            t.Deadline.Value.Date <= today.AddDays(3) &&
            (t.StatusID == 1 || t.StatusID == 4));

        // Загрузка отделов
        var departmentLoads = await _context.Departments
            .Select(d => new DepartmentLoadDto
            {
                DepartmentID = d.DepartmentID,
                DepartmentName = d.Name,
                TotalEmployees = _context.Employees.Count(e => e.DepartmentID == d.DepartmentID && e.IsActive),
                CombinedEmployees = _context.Employees.Count(e => e.DepartmentID == d.DepartmentID && e.IsActive && e.IsCombined)
            })
            .ToListAsync();

        // Распределение поручений по статусам
        var taskDistribution = await _context.TaskStatuses
            .Select(ts => new TaskStatusDistributionDto
            {
                StatusName = ts.Name,
                ColorHex = ts.ColorHex,
                Count = _context.Tasks.Count(t => t.StatusID == ts.StatusID)
            })
            .ToListAsync();

        // Ближайшие занятия
        var upcomingClasses = await _context.Schedule
            .Include(s => s.Teacher)
            .Where(s => s.IsActive && s.StartDate >= today)
            .OrderBy(s => s.StartDate)
            .ThenBy(s => s.StartTime)
            .Take(5)
            .Select(s => new UpcomingClassDto
            {
                Title = s.Title,
                TeacherName = s.Teacher.LastName + " " + s.Teacher.FirstName,
                StartDate = s.StartDate,
                StartTime = s.StartTime,
                Room = s.Room
            })
            .ToListAsync();

        return new DashboardDto
        {
            TotalEmployees = totalEmployees,
            ActiveSchedules = activeSchedules,
            TodayClasses = todayClasses,
            TasksInProgress = tasksInProgress,
            TasksOverdue = tasksOverdue,
            TasksNearDeadline = tasksNearDeadline,
            DepartmentLoads = departmentLoads,
            TaskDistribution = taskDistribution,
            UpcomingClasses = upcomingClasses
        };
    }
}