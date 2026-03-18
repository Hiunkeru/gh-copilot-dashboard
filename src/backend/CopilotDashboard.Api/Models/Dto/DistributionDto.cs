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
    public int LocAdded { get; set; }
    public int LocSuggestedToAdd { get; set; }
    public int InteractionCount { get; set; }
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

public class UserDayDetailDto
{
    public string Date { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int InteractionCount { get; set; }
    public int CodeGenerationCount { get; set; }
    public int CodeAcceptanceCount { get; set; }
    public int LocSuggestedToAdd { get; set; }
    public int LocAdded { get; set; }
    public int LocSuggestedToDelete { get; set; }
    public int LocDeleted { get; set; }
    public bool UsedChat { get; set; }
    public bool UsedAgent { get; set; }
    public bool UsedCli { get; set; }
    public int ChatAgentModeCount { get; set; }
    public int ChatAskModeCount { get; set; }
    public int ChatEditModeCount { get; set; }
    public decimal AcceptanceRate { get; set; }
    public string? PrimaryEditor { get; set; }
    public string? PrimaryLanguage { get; set; }
    // Feature breakdown for this day
    public List<FeatureBreakdownDto> Features { get; set; } = new();
    // Language breakdown for this day
    public List<LanguageBreakdownDto> Languages { get; set; } = new();
}

public class FeatureBreakdownDto
{
    public string Feature { get; set; } = string.Empty;
    public int CodeGenerationCount { get; set; }
    public int CodeAcceptanceCount { get; set; }
    public int LocAdded { get; set; }
}

public class LanguageBreakdownDto
{
    public string Language { get; set; } = string.Empty;
    public string? Feature { get; set; }
    public int CodeGenerationCount { get; set; }
    public int CodeAcceptanceCount { get; set; }
    public int LocSuggestedToAdd { get; set; }
    public int LocAdded { get; set; }
}
