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

    [HttpGet]
    public async Task<IActionResult> GetReports(CancellationToken ct)
    {
        var reports = await _reportService.GetReportsAsync(ct);
        return Ok(reports);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReport(int id, CancellationToken ct)
    {
        var report = await _reportService.GetReportByIdAsync(id, ct);
        if (report is null) return NotFound();
        return Ok(report);
    }
}
