using System.Text.Json.Serialization;

namespace CopilotDashboard.Api.Models.GitHub;

public class MetricsReportResponse
{
    [JsonPropertyName("download_links")]
    public List<string> DownloadLinks { get; set; } = new();

    [JsonPropertyName("report_start_day")]
    public string? ReportStartDay { get; set; }

    [JsonPropertyName("report_end_day")]
    public string? ReportEndDay { get; set; }
}
