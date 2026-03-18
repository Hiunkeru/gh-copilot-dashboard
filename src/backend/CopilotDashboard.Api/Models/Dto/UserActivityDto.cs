namespace CopilotDashboard.Api.Models.Dto;

public class UserActivityDto
{
    public string UserLogin { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Organization { get; set; }
    public string? Team { get; set; }
    public string? LastActivity { get; set; }
    public int ActiveDays { get; set; }
    public int TotalSuggestions { get; set; }
    public int TotalAcceptances { get; set; }
    public decimal AcceptanceRate { get; set; }
    public bool UsesChat { get; set; }
    public bool UsesAgent { get; set; }
    public bool UsesCli { get; set; }
    public string Category { get; set; } = string.Empty; // PowerUser, Occasional, Inactive, NeverUsed
}

public class UserActivityPageDto
{
    public List<UserActivityDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
