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

    public User User { get; set; } = null!;
}
