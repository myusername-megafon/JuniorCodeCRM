using JuniorCodeCRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JuniorCodeCRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Получить данные для дашборда (ключевые метрики)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return Ok(dashboard);
    }
}