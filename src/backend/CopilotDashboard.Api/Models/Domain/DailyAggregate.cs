namespace CopilotDashboard.Api.Models.Domain;

public class DailyAggregate
{
    public DateOnly Date { get; set; }
    public int TotalActiveUsers { get; set; }
    public int TotalEngagedUsers { get; set; }
    public int TotalSuggestions { get; set; }
    public int TotalAcceptances { get; set; }
    public decimal AcceptanceRate { get; set; }
}
