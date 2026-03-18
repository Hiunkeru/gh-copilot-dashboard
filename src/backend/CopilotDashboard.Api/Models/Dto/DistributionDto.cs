namespace CopilotDashboard.Api.Models.Dto;

public class DistributionItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Suggestions { get; set; }
    public int Acceptances { get; set; }
    public int LinesAccepted { get; set; }
    public int UserCount { get; set; }
}

public class TrendPointDto
{
    public string Date { get; set; } = string.Empty;
    public int ActiveUsers { get; set; }
    public int EngagedUsers { get; set; }
    public int Suggestions { get; set; }
    public int Acceptances { get; set; }
    public decimal AcceptanceRate { get; set; }
}

public class RoiDto
{
    public int TotalLinesAccepted { get; set; }
    public int TotalLinesAcceptedLast7Days { get; set; }
    public decimal AvgLinesPerActiveUserPerDay { get; set; }
    public decimal CostPerActiveUser { get; set; }
    public decimal LicenseCostPerMonth { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalSeats { get; set; }
}
