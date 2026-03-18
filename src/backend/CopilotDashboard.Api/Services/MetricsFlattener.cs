using CopilotDashboard.Api.Models.Domain;
using CopilotDashboard.Api.Models.GitHub;

namespace CopilotDashboard.Api.Services;

public class MetricsFlattener : IMetricsFlattener
{
    public (DailyUsage Usage, List<DailyUsageDetail> Details)? Flatten(UserMetricsRecord record)
    {
        var dateStr = record.EffectiveDate;
        if (string.IsNullOrWhiteSpace(dateStr) || !DateOnly.TryParse(dateStr, out var date))
            return null;
        if (string.IsNullOrWhiteSpace(record.UserLogin))
            return null;

        var details = new List<DailyUsageDetail>();

        // Check if this is the new format (totals_by_ide) or legacy format
        if (record.TotalsByIde is { Count: > 0 })
        {
            return FlattenNewFormat(record, date, details);
        }
        else if (record.CopilotIdeCodeCompletions is not null)
        {
            return FlattenLegacyFormat(record, date, details);
        }

        // Minimal record — user exists but no IDE data
        var usage = new DailyUsage
        {
            UserLogin = record.UserLogin,
            Date = date,
            IsActive = record.IsActive,
            IsEngaged = record.IsEngagedUser == true,
        };
        return (usage, details);
    }

    private (DailyUsage, List<DailyUsageDetail>) FlattenNewFormat(
        UserMetricsRecord record, DateOnly date, List<DailyUsageDetail> details)
    {
        int totalSuggestions = 0, totalAcceptances = 0;
        int totalLinesSuggested = 0, totalLinesAccepted = 0;
        var editors = new Dictionary<string, int>();
        var languages = new Dictionary<string, int>();
        bool chatEngaged = false, agentEngaged = false;

        foreach (var ide in record.TotalsByIde!)
        {
            totalSuggestions += ide.CodeGenerationActivityCount;
            totalAcceptances += ide.CodeAcceptanceActivityCount;
            editors[ide.Ide] = editors.GetValueOrDefault(ide.Ide) + ide.CodeGenerationActivityCount;

            // Check features
            if (ide.TotalsByFeature is not null)
            {
                foreach (var feat in ide.TotalsByFeature)
                {
                    if (feat.Feature.Contains("chat", StringComparison.OrdinalIgnoreCase) && feat.UserInitiatedInteractionCount > 0)
                        chatEngaged = true;
                    if (feat.Feature.Contains("agent", StringComparison.OrdinalIgnoreCase) && feat.UserInitiatedInteractionCount > 0)
                        agentEngaged = true;
                }
            }

            // Language details
            if (ide.TotalsByLanguage is not null)
            {
                foreach (var lang in ide.TotalsByLanguage)
                {
                    languages[lang.Language] = languages.GetValueOrDefault(lang.Language) + lang.CodeGenerationActivityCount;
                    totalLinesSuggested += lang.CodeLinesGeneratedCount;
                    totalLinesAccepted += lang.CodeLinesAcceptedCount;

                    details.Add(new DailyUsageDetail
                    {
                        UserLogin = record.UserLogin,
                        Date = date,
                        EditorName = ide.Ide,
                        LanguageName = lang.Language,
                        Suggestions = lang.CodeGenerationActivityCount,
                        Acceptances = lang.CodeAcceptanceActivityCount,
                        LinesSuggested = lang.CodeLinesGeneratedCount,
                        LinesAccepted = lang.CodeLinesAcceptedCount,
                    });
                }
            }
        }

        // Consolidate details with same key
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
            IsActive = record.IsActive,
            IsEngaged = totalAcceptances > 0 || record.UserInitiatedInteractionCount > 0,
            CompletionsSuggestions = totalSuggestions,
            CompletionsAcceptances = totalAcceptances,
            CompletionsLinesSuggested = totalLinesSuggested,
            CompletionsLinesAccepted = totalLinesAccepted,
            ChatEngaged = chatEngaged,
            AgentEngaged = agentEngaged,
            CliEngaged = false,
            PrimaryEditor = primaryEditor,
            PrimaryLanguage = primaryLanguage,
        };

        return (usage, details);
    }

    private (DailyUsage, List<DailyUsageDetail>) FlattenLegacyFormat(
        UserMetricsRecord record, DateOnly date, List<DailyUsageDetail> details)
    {
        int totalSuggestions = 0, totalAcceptances = 0;
        int totalLinesSuggested = 0, totalLinesAccepted = 0;
        var editors = new Dictionary<string, int>();
        var languages = new Dictionary<string, int>();

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
            IsActive = record.IsActiveUser == true || totalSuggestions > 0,
            IsEngaged = record.IsEngagedUser == true || totalAcceptances > 0,
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
