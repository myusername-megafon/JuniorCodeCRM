using JuniorCodeCRM.Models.DTOs.Report;

namespace JuniorCodeCRM.Services.Interfaces;

public interface IReportService
{
    Task<List<StaffByDepartmentDto>> GetStaffByDepartmentReportAsync();
    Task<List<TaskExecutionDto>> GetTaskExecutionReportAsync();
    Task<List<TeacherLoadDto>> GetTeacherLoadReportAsync();
}