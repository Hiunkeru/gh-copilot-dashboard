using CopilotDashboard.Api.Models.GitHub;

namespace CopilotDashboard.Api.Services;

public interface IGitHubCopilotService
{
    Task<MetricsReportResponse> GetUserMetrics28DayAsync(CancellationToken ct = default);
    Task<MetricsReportResponse> GetEnterpriseMetrics28DayAsync(CancellationToken ct = default);
    Task<MetricsReportResponse> GetUserMetricsDayAsync(DateOnly day, CancellationToken ct = default);
    IAsyncEnumerable<UserMetricsRecord> StreamNdjsonRecordsAsync(IEnumerable<string> downloadLinks, CancellationToken ct = default);
    Task<List<string>> GetEnterpriseOrgsAsync(CancellationToken ct = default);
    Task<List<SeatInfo>> GetSeatsForOrgAsync(string org, CancellationToken ct = default);
    Task<List<SeatInfo>> GetAllSeatsAsync(CancellationToken ct = default);
}
