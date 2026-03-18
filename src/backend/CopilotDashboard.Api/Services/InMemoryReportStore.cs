using CopilotDashboard.Api.Models.Domain;

namespace CopilotDashboard.Api.Services;

/// <summary>
/// In-memory report store for local development.
/// </summary>
public class InMemoryReportStore : IReportStore
{
    private readonly List<Report> _reports = new();
    private int _nextId = 1;

    public Task SaveAsync(Report report, CancellationToken ct = default)
    {
        report.Id = _nextId++;
        _reports.Add(report);
        return Task.CompletedTask;
    }

    public Task<List<Report>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_reports.OrderByDescending(r => r.GeneratedAt).ToList());
    }

    public Task<Report?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return Task.FromResult(_reports.FirstOrDefault(r => r.Id == id));
    }
}
