namespace CopilotDashboard.Api.Models.Dto;

public class AdoptionReportDto
{
    public string GeneratedAt { get; set; } = string.Empty;
    public string PeriodStart { get; set; } = string.Empty;
    public string PeriodEnd { get; set; } = string.Empty;

    // Structured sections
    public ReportSectionDto ExecutiveSummary { get; set; } = new();
    public ReportSectionDto AdoptionAnalysis { get; set; } = new();
    public ReportSectionDto TopPerformers { get; set; } = new();
    public ReportSectionDto AtRiskUsers { get; set; } = new();
    public ReportSectionDto FeatureAdoption { get; set; } = new();
    public ReportSectionDto Trends { get; set; } = new();
    public ReportSectionDto Recommendations { get; set; } = new();
    public ReportSectionDto RoiAnalysis { get; set; } = new();

    // Raw markdown for full report view
    public string FullReportMarkdown { get; set; } = string.Empty;
}

public class ReportSectionDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
