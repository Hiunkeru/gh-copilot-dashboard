using System.Runtime.CompilerServices;
using System.Text.Json;
using CopilotDashboard.Api.Configuration;
using CopilotDashboard.Api.Models.GitHub;
using Microsoft.Extensions.Options;

namespace CopilotDashboard.Api.Services;

public class GitHubCopilotService : IGitHubCopilotService
{
    private readonly HttpClient _httpClient;
    private readonly GitHubOptions _options;
    private readonly ILogger<GitHubCopilotService> _logger;

    public GitHubCopilotService(
        HttpClient httpClient,
        IOptions<GitHubOptions> options,
        ILogger<GitHubCopilotService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.github.com/");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.Token}");
        _httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", _options.ApiVersion);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "CopilotDashboard/1.0");
    }

    public async Task<MetricsReportResponse> GetUserMetrics28DayAsync(CancellationToken ct = default)
    {
        var url = $"enterprises/{_options.Enterprise}/copilot/metrics/reports/users-28-day/latest";
        _logger.LogInformation("Fetching user metrics 28-day from {Url}", url);

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<MetricsReportResponse>(ct);
        _logger.LogInformation("Received {Count} download links for period {Start} to {End}",
            result?.DownloadLinks.Count, result?.ReportStartDay, result?.ReportEndDay);

        return result ?? throw new InvalidOperationException("Empty response from GitHub API");
    }

    public async Task<MetricsReportResponse> GetEnterpriseMetrics28DayAsync(CancellationToken ct = default)
    {
        var url = $"enterprises/{_options.Enterprise}/copilot/metrics/reports/enterprise-28-day/latest";
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MetricsReportResponse>(ct)
            ?? throw new InvalidOperationException("Empty response from GitHub API");
    }

    public async Task<MetricsReportResponse> GetUserMetricsDayAsync(DateOnly day, CancellationToken ct = default)
    {
        var url = $"enterprises/{_options.Enterprise}/copilot/metrics/reports/users-1-day?day={day:yyyy-MM-dd}";
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MetricsReportResponse>(ct)
            ?? throw new InvalidOperationException("Empty response from GitHub API");
    }

    public async IAsyncEnumerable<UserMetricsRecord> StreamNdjsonRecordsAsync(
        IEnumerable<string> downloadLinks,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var link in downloadLinks)
        {
            _logger.LogDebug("Downloading NDJSON from {Link}", link);

            using var response = await _httpClient.GetAsync(link,
                HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                UserMetricsRecord? record;
                try
                {
                    record = JsonSerializer.Deserialize<UserMetricsRecord>(line);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse NDJSON line: {Line}", line[..Math.Min(100, line.Length)]);
                    continue;
                }

                if (record is not null)
                    yield return record;
            }
        }
    }

    public async Task<List<SeatInfo>> GetSeatsAsync(CancellationToken ct = default)
    {
        var seats = new List<SeatInfo>();
        var page = 1;

        while (true)
        {
            var url = $"orgs/{_options.Organization}/copilot/billing/seats?page={page}&per_page=100";
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<SeatsResponse>(ct);
            if (data?.Seats is null || data.Seats.Count == 0) break;

            seats.AddRange(data.Seats);

            if (seats.Count >= data.TotalSeats) break;
            page++;
        }

        _logger.LogInformation("Fetched {Count} seats", seats.Count);
        return seats;
    }
}
