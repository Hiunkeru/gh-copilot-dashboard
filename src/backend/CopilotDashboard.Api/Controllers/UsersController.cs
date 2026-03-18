using CopilotDashboard.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CopilotDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public UsersController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = true,
        CancellationToken ct = default)
    {
        var result = await _dashboardService.GetUsersAsync(page, pageSize, search, category, sortBy, sortDesc, ct);
        return Ok(result);
    }

    [HttpGet("{userLogin}/history")]
    public async Task<IActionResult> GetUserHistory(string userLogin, CancellationToken ct)
    {
        var result = await _dashboardService.GetUserHistoryAsync(userLogin, ct);
        return Ok(result);
    }
}
