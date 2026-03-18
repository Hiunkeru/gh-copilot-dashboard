using CopilotDashboard.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CopilotDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IAdoptionReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IAdoptionReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport(CancellationToken ct)
    {
        _logger.LogInformation("Report generation requested by {User}", User.Identity?.Name);
        var report = await _reportService.GenerateReportAsync(ct);
        return Ok(report);
    }
}
