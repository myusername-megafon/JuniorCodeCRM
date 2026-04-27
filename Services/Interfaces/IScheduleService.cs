using JuniorCodeCRM.Models.DTOs.Schedule;

namespace JuniorCodeCRM.Services.Interfaces;

public interface IScheduleService
{
    Task<List<ScheduleDto>> GetAllAsync(ScheduleFilterDto filter);
    Task<ScheduleDto?> GetByIdAsync(int id);
    Task<ScheduleDto> CreateAsync(ScheduleCreateDto dto, string? ipAddress);
    Task<ScheduleDto?> UpdateAsync(int id, ScheduleUpdateDto dto, string? ipAddress);
    Task<bool> DeleteAsync(int id, string? ipAddress);
    Task<int> GetTotalCountAsync(ScheduleFilterDto filter);
}