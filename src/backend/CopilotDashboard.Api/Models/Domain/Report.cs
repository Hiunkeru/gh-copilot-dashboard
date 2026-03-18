namespace CopilotDashboard.Api.Models.Domain;

public class Report
{
    public int Id { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string PeriodStart { get; set; } = string.Empty;
    public string PeriodEnd { get; set; } = string.Empty;
    public string FullReportMarkdown { get; set; } = string.Empty;

    // Denormalized summary for the list view
    public int TotalSeats { get; set; }
    public int ActiveUsers { get; set; }
    public decimal AdoptionRate { get; set; }
    public decimal AcceptanceRate { get; set; }
    public string GeneratedBy { get; set; } = "system";
}
