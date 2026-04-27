using JuniorCodeCRM.Models.Enums;

namespace JuniorCodeCRM.Services.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportStaffByDepartmentAsync(ExportFormat format);
    Task<byte[]> ExportTaskExecutionAsync(ExportFormat format);
    Task<byte[]> ExportTeacherLoadAsync(ExportFormat format);
}