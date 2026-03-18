namespace CopilotDashboard.Api.Models.Dto;

public class FeatureUsageDto
{
    public int CompletionsUsers { get; set; }
    public int ChatUsers { get; set; }
    public int AgentUsers { get; set; }
    public int CliUsers { get; set; }
    public int TotalUsers { get; set; }
    public decimal CompletionsPercent { get; set; }
    public decimal ChatPercent { get; set; }
    public decimal AgentPercent { get; set; }
    public decimal CliPercent { get; set; }
}
