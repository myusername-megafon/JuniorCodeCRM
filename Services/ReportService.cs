using JuniorCodeCRM.Data;
using JuniorCodeCRM.Models.DTOs.Report;
using JuniorCodeCRM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JuniorCodeCRM.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Отчёт «Кадровый состав по отделам» — группировка по отделам и должностям, суммирование
    /// </summary>
    public async Task<List<StaffByDepartmentDto>> GetStaffByDepartmentReportAsync()
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Where(e => e.IsActive)
            .GroupBy(e => new { DepartmentName = e.Department.Name, PositionName = e.Position.Name })
            .Select(g => new StaffByDepartmentDto
            {
                Department = g.Key.DepartmentName,
                Position = g.Key.PositionName,
                EmployeeCount = g.Count(),
                CombinedCount = g.Count(e => e.IsCombined)
            })
            .OrderBy(r => r.Department)
            .ThenBy(r => r.Position)
            .ToListAsync();
    }

    /// <summary>
    /// Отчёт «Исполнение поручений» — группировка по статусам и исполнителям, суммирование
    /// </summary>
    public async Task<List<TaskExecutionDto>> GetTaskExecutionReportAsync()
    {
        var today = DateTime.Now.Date;

        return await _context.Tasks
            .Include(t => t.Status)
            .Include(t => t.Assignee)
            .GroupBy(t => new { t.Status.Name, AssigneeName = t.Assignee.LastName + " " + t.Assignee.FirstName })
            .Select(g => new TaskExecutionDto
            {
                Status = g.Key.Name,
                Assignee = g.Key.AssigneeName,
                TaskCount = g.Count(),
                OverdueCount = g.Count(t =>
                    t.Deadline != null &&
                    t.Deadline.Value.Date < today &&
                    (t.StatusID == 1 || t.StatusID == 4)),
                CompletionPercent = g.Key.Name == "Выполнено"
                    ? 100m
                    : Math.Round(
                        (decimal)g.Count(t => t.StatusID == 2) / g.Count() * 100,
                        1
                      )
            })
            .OrderBy(r => r.Status)
            .ThenBy(r => r.Assignee)
            .ToListAsync();
    }

    /// <summary>
    /// Отчёт «Загрузка преподавателей» — группировка по преподавателям, суммирование часов
    /// </summary>
    public async Task<List<TeacherLoadDto>> GetTeacherLoadReportAsync()
    {
        return await _context.Schedule
            .Include(s => s.Teacher)
                .ThenInclude(t => t.Department)
            .Where(s => s.IsActive)
            .GroupBy(s => new
            {
                TeacherName = s.Teacher.LastName + " " + s.Teacher.FirstName +
                    (s.Teacher.MiddleName != null ? " " + s.Teacher.MiddleName : ""),
                s.Teacher.Department.Name
            })
            .Select(g => new TeacherLoadDto
            {
                TeacherName = g.Key.TeacherName,
                Department = g.Key.Name,
                TotalClasses = g.Count(),
                TotalMinutes = g.Sum(s => s.Duration),
                TotalHours = Math.Round((decimal)g.Sum(s => s.Duration) / 60, 1),
                RecurringClasses = g.Count(s => s.IsRecurring)
            })
            .OrderByDescending(r => r.TotalHours)
            .ToListAsync();
    }
}