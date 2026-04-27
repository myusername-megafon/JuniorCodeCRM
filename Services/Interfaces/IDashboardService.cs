using JuniorCodeCRM.Models.DTOs.Dashboard;

namespace JuniorCodeCRM.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}