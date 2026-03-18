namespace CopilotDashboard.Api.Models.Dto;

public class AdoptionOverviewDto
{
    public int TotalSeats { get; set; }
    public int ActiveUsers { get; set; }
    public int EngagedUsers { get; set; }
    public decimal AdoptionRate { get; set; }
    public int DailyActiveUsers { get; set; }
    public int WeeklyActiveUsers { get; set; }
    public int WastedSeats { get; set; }
    public int TotalSuggestions { get; set; }
    public int TotalAcceptances { get; set; }
    public decimal AcceptanceRate { get; set; }
    public int TotalLinesAccepted { get; set; }
    public string? DataAsOf { get; set; }
}
