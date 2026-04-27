using JuniorCodeCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JuniorCodeCRM.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Используем полные имена для избежания конфликта с System.Threading.Tasks.TaskStatus
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<JuniorCodeCRM.Models.Entities.TaskStatus> TaskStatuses => Set<JuniorCodeCRM.Models.Entities.TaskStatus>();
    public DbSet<TaskPriority> TaskPriorities => Set<TaskPriority>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<ScheduleItem> Schedule => Set<ScheduleItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasOne(emp => emp.Position)
             .WithMany()
             .HasForeignKey(emp => emp.PositionID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(emp => emp.CombinedPosition)
             .WithMany()
             .HasForeignKey(emp => emp.CombinedPositionID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(emp => emp.Department)
             .WithMany()
             .HasForeignKey(emp => emp.DepartmentID)
             .OnDelete(DeleteBehavior.Restrict);

            // Индексы
            e.HasIndex(emp => emp.LastName);

            // HasFilter не поддерживается в некоторых версиях EF Core, убираем фильтр
            e.HasIndex(emp => emp.IsActive);
        });

        // TaskItem
        modelBuilder.Entity<TaskItem>(t =>
        {
            t.HasOne(task => task.Assignee)
             .WithMany(e => e.Tasks)
             .HasForeignKey(task => task.AssigneeID)
             .OnDelete(DeleteBehavior.Restrict);

            t.HasOne(task => task.Status)
             .WithMany()
             .HasForeignKey(task => task.StatusID)
             .OnDelete(DeleteBehavior.Restrict);

            t.HasOne(task => task.Priority)
             .WithMany()
             .HasForeignKey(task => task.PriorityID)
             .OnDelete(DeleteBehavior.Restrict);

            // Индексы
            t.HasIndex(task => task.StatusID);
            t.HasIndex(task => task.AssigneeID);
            t.HasIndex(task => task.Deadline);
        });

        // ScheduleItem
        modelBuilder.Entity<ScheduleItem>(s =>
        {
            s.HasOne(sch => sch.Teacher)
             .WithMany(e => e.ScheduleItems)
             .HasForeignKey(sch => sch.TeacherID)
             .OnDelete(DeleteBehavior.Restrict);

            // Индексы (HasFilter убран)
            s.HasIndex(sch => sch.TeacherID);
            s.HasIndex(sch => sch.StartDate);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(a =>
        {
            // Индекс по дате (сортировка DESC)
            a.HasIndex(log => log.ActionDate).IsDescending();
        });
    }
}