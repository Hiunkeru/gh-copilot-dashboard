namespace CopilotDashboard.Api.Configuration;

public class GitHubOptions
{
    public const string SectionName = "GitHub";

    public string Enterprise { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2026-03-10";
    public decimal LicenseCostPerMonth { get; set; } = 19m; // USD per seat/month for Business
}

public class SyncOptions
{
    public const string SectionName = "Sync";

    public string CronSchedule { get; set; } = "0 6 * * *"; // 6 AM UTC daily
    public bool Enabled { get; set; } = true;
}
