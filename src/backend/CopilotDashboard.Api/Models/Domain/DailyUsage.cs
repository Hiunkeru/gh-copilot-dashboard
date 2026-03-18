namespace CopilotDashboard.Api.Models.Domain;

public class DailyUsage
{
    public string UserLogin { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public bool IsActive { get; set; }
    public bool IsEngaged { get; set; }
    public int CompletionsSuggestions { get; set; }
    public int CompletionsAcceptances { get; set; }
    public int CompletionsLinesSuggested { get; set; }
    public int CompletionsLinesAccepted { get; set; }
    public bool ChatEngaged { get; set; }
    public bool AgentEngaged { get; set; }
    public bool CliEngaged { get; set; }
    public string? PrimaryEditor { get; set; }
    public string? PrimaryLanguage { get; set; }

    // New fields from real API
    public int LocSuggestedToAdd { get; set; }
    public int LocSuggestedToDelete { get; set; }
    public int LocAdded { get; set; }
    public int LocDeleted { get; set; }
    public int InteractionCount { get; set; }
    public int CodeGenerationCount { get; set; }
    public int CodeAcceptanceCount { get; set; }
    public bool UsedChat { get; set; }
    public bool UsedAgent { get; set; }
    public bool UsedCli { get; set; }
    public int ChatAgentModeCount { get; set; }
    public int ChatAskModeCount { get; set; }
    public int ChatEditModeCount { get; set; }
    public int CliSessionCount { get; set; }
    public int CliRequestCount { get; set; }
    public int CliPromptTokens { get; set; }
    public int CliOutputTokens { get; set; }

    public User User { get; set; } = null!;
}
