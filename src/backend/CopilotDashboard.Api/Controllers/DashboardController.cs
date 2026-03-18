using CopilotDashboard.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CopilotDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken ct)
        => Ok(await _dashboardService.GetOverviewAsync(ct));

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends([FromQuery] int days = 28, CancellationToken ct = default)
        => Ok(await _dashboardService.GetTrendsAsync(days, ct));

    [HttpGet("features")]
    public async Task<IActionResult> GetFeatures([FromQuery] int days = 28, CancellationToken ct = default)
        => Ok(await _dashboardService.GetFeatureUsageAsync(days, ct));

    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages([FromQuery] int days = 28, CancellationToken ct = default)
        => Ok(await _dashboardService.GetLanguageDistributionAsync(days, ct));

    [HttpGet("editors")]
    public async Task<IActionResult> GetEditors([FromQuery] int days = 28, CancellationToken ct = default)
        => Ok(await _dashboardService.GetEditorDistributionAsync(days, ct));

    [HttpGet("roi")]
    public async Task<IActionResult> GetRoi(CancellationToken ct)
        => Ok(await _dashboardService.GetRoiAsync(ct));
}
