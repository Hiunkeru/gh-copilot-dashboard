using CopilotDashboard.Api.Models.Domain;
using CopilotDashboard.Api.Models.GitHub;

namespace CopilotDashboard.Api.Services;

public class MetricsFlattener : IMetricsFlattener
{
    public (DailyUsage Usage, List<DailyUsageDetail> Details)? Flatten(UserMetricsRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.Date) || !DateOnly.TryParse(record.Date, out var date))
            return null;
        if (string.IsNullOrWhiteSpace(record.UserLogin))
            return null;
        var details = new List<DailyUsageDetail>();

        int totalSuggestions = 0, totalAcceptances = 0;
        int totalLinesSuggested = 0, totalLinesAccepted = 0;
        var editors = new Dictionary<string, int>(); // editor -> suggestions count
        var languages = new Dictionary<string, int>(); // language -> suggestions count

        if (record.CopilotIdeCodeCompletions?.Editors is { } editorList)
        {
            foreach (var editor in editorList)
            {
                foreach (var model in editor.Models ?? [])
                {
                    foreach (var lang in model.Languages ?? [])
                    {
                        totalSuggestions += lang.TotalCodeSuggestions;
                        totalAcceptances += lang.TotalCodeAcceptances;
                        totalLinesSuggested += lang.TotalCodeLinesSuggested;
                        totalLinesAccepted += lang.TotalCodeLinesAccepted;

                        editors[editor.Name] = editors.GetValueOrDefault(editor.Name) + lang.TotalCodeSuggestions;
                        languages[lang.Name] = languages.GetValueOrDefault(lang.Name) + lang.TotalCodeSuggestions;

                        details.Add(new DailyUsageDetail
                        {
                            UserLogin = record.UserLogin,
                            Date = date,
                            EditorName = editor.Name,
                            LanguageName = lang.Name,
                            Suggestions = lang.TotalCodeSuggestions,
                            Acceptances = lang.TotalCodeAcceptances,
                            LinesSuggested = lang.TotalCodeLinesSuggested,
                            LinesAccepted = lang.TotalCodeLinesAccepted,
                        });
                    }
                }
            }
        }

        // Consolidate details with same user+date+editor+language (from different models)
        details = details
            .GroupBy(d => new { d.UserLogin, d.Date, d.EditorName, d.LanguageName })
            .Select(g => new DailyUsageDetail
            {
                UserLogin = g.Key.UserLogin,
                Date = g.Key.Date,
                EditorName = g.Key.EditorName,
                LanguageName = g.Key.LanguageName,
                Suggestions = g.Sum(x => x.Suggestions),
                Acceptances = g.Sum(x => x.Acceptances),
                LinesSuggested = g.Sum(x => x.LinesSuggested),
                LinesAccepted = g.Sum(x => x.LinesAccepted),
            })
            .ToList();

        var primaryEditor = editors.OrderByDescending(kv => kv.Value).FirstOrDefault().Key ?? "unknown";
        var primaryLanguage = languages.OrderByDescending(kv => kv.Value).FirstOrDefault().Key ?? "unknown";

        var usage = new DailyUsage
        {
            UserLogin = record.UserLogin,
            Date = date,
            IsActive = record.IsActiveUser,
            IsEngaged = record.IsEngagedUser,
            CompletionsSuggestions = totalSuggestions,
            CompletionsAcceptances = totalAcceptances,
            CompletionsLinesSuggested = totalLinesSuggested,
            CompletionsLinesAccepted = totalLinesAccepted,
            ChatEngaged = record.CopilotIdeChat?.IsEngagedUser ?? false,
            AgentEngaged = record.CopilotIdeAgent?.IsEngagedUser ?? false,
            CliEngaged = record.TotalsByCli?.IsEngagedUser ?? false,
            PrimaryEditor = primaryEditor,
            PrimaryLanguage = primaryLanguage,
        };

        return (usage, details);
    }
}
