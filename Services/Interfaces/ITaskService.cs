using JuniorCodeCRM.Models.DTOs.Task;

namespace JuniorCodeCRM.Services.Interfaces;

public interface ITaskService
{
    Task<List<TaskDto>> GetAllAsync(TaskFilterDto filter);
    Task<TaskDto?> GetByIdAsync(int id);
    Task<TaskDto> CreateAsync(TaskCreateDto dto, string? ipAddress);
    Task<TaskDto?> UpdateAsync(int id, TaskUpdateDto dto, string? ipAddress);
    Task<TaskDto?> MoveAsync(TaskMoveDto dto, string? ipAddress);
    Task<bool> DeleteAsync(int id, string? ipAddress);
    Task<int> GetTotalCountAsync(TaskFilterDto filter);
}