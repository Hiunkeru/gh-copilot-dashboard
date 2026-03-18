using CopilotDashboard.Api.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CopilotDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly DailyMetricsSyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(DailyMetricsSyncService syncService, ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerSync(CancellationToken ct)
    {
        _logger.LogInformation("Manual sync triggered by {User}", User.Identity?.Name);
        await _syncService.RunSyncAsync(ct);
        return Ok(new { message = "Sync completed successfully" });
    }
}
