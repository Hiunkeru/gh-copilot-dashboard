using CopilotDashboard.Api.BackgroundServices;
using CopilotDashboard.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CopilotDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly DailyMetricsSyncService _syncService;
    private readonly IGitHubCopilotService _githubService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(
        DailyMetricsSyncService syncService,
        IGitHubCopilotService githubService,
        ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _githubService = githubService;
        _logger = logger;
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> TriggerSync(CancellationToken ct)
    {
        _logger.LogInformation("Manual sync triggered by {User}", User.Identity?.Name);
        await _syncService.RunSyncAsync(ct);
        return Ok(new { message = "Sync completed successfully" });
    }

    /// <summary>
    /// Debug endpoint: returns raw NDJSON samples from the GitHub API.
    /// </summary>
    [HttpGet("debug/raw-sample")]
    public async Task<IActionResult> GetRawSample([FromQuery] int count = 3, CancellationToken ct = default)
    {
        var report = await _githubService.GetUserMetrics28DayAsync(ct);

        using var plainClient = new HttpClient();
        var samples = new List<object>();

        foreach (var link in report.DownloadLinks)
        {
            var response = await plainClient.GetStringAsync(link, ct);
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines.Take(count))
            {
                var doc = System.Text.Json.JsonDocument.Parse(line);
                samples.Add(doc.RootElement);
            }

            if (samples.Count >= count) break;
        }

        return Ok(new
        {
            reportStartDay = report.ReportStartDay,
            reportEndDay = report.ReportEndDay,
            downloadLinksCount = report.DownloadLinks.Count,
            sampleCount = samples.Count,
            samples
        });
    }
}
