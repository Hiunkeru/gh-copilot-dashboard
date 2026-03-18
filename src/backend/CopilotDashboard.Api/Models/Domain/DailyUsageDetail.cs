namespace CopilotDashboard.Api.Models.Domain;

public class DailyUsageDetail
{
    public string UserLogin { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string EditorName { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public int Suggestions { get; set; }
    public int Acceptances { get; set; }
    public int LinesSuggested { get; set; }
    public int LinesAccepted { get; set; }

    public User User { get; set; } = null!;
}
