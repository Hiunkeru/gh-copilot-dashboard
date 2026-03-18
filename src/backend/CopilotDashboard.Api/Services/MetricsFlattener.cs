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

        // Check if this is the new format (totals_by_ide / totals_by_feature) or legacy format
        if (record.TotalsByIde is { Count: > 0 } || record.TotalsByFeature is { Count: > 0 })
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
            InteractionCount = record.UserInitiatedInteractionCount,
            CodeGenerationCount = record.CodeGenerationActivityCount,
            CodeAcceptanceCount = record.CodeAcceptanceActivityCount,
            LocSuggestedToAdd = record.LocSuggestedToAddSum,
            LocSuggestedToDelete = record.LocSuggestedToDeleteSum,
            LocAdded = record.LocAddedSum,
            LocDeleted = record.LocDeletedSum,
            UsedChat = record.UsedChat,
            UsedAgent = record.UsedAgent,
            UsedCli = record.UsedCli,
            ChatAgentModeCount = record.ChatPanelAgentMode ?? 0,
            ChatAskModeCount = record.ChatPanelAskMode ?? 0,
            ChatEditModeCount = record.ChatPanelEditMode ?? 0,
        };

        PopulateCliFields(usage, record);

        return (usage, details);
    }

    private (DailyUsage, List<DailyUsageDetail>) FlattenNewFormat(
        UserMetricsRecord record, DateOnly date, List<DailyUsageDetail> details)
    {
        int totalSuggestions = 0, totalAcceptances = 0;
        int totalLinesSuggested = 0, totalLinesAccepted = 0;
        var editors = new Dictionary<string, int>();
        var languages = new Dictionary<string, int>();

        // Process totals_by_ide for editor breakdown and detail rows
        if (record.TotalsByIde is not null)
        {
            foreach (var ide in record.TotalsByIde)
            {
                totalSuggestions += ide.CodeGenerationActivityCount;
                totalAcceptances += ide.CodeAcceptanceActivityCount;
                editors[ide.Ide] = editors.GetValueOrDefault(ide.Ide) + ide.CodeGenerationActivityCount;

                // If the IDE entry has nested language data (older new-format variant)
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
        }

        // Process totals_by_language_feature for language breakdown detail rows
        if (record.TotalsByLanguageFeature is { Count: > 0 })
        {
            foreach (var lf in record.TotalsByLanguageFeature)
            {
                languages[lf.Language] = languages.GetValueOrDefault(lf.Language) + lf.CodeGenerationActivityCount;

                // Determine the editor from totals_by_ide (use primary or "unknown")
                var editorName = record.TotalsByIde?.FirstOrDefault()?.Ide ?? "unknown";

                details.Add(new DailyUsageDetail
                {
                    UserLogin = record.UserLogin,
                    Date = date,
                    EditorName = editorName,
                    LanguageName = lf.Language,
                    Suggestions = lf.CodeGenerationActivityCount,
                    Acceptances = lf.CodeAcceptanceActivityCount,
                    LinesSuggested = lf.LocSuggestedToAddSum,
                    LinesAccepted = lf.LocAddedSum,
                });
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

        // Determine chat/agent engaged from totals_by_feature or used_* flags
        bool chatEngaged = record.UsedChat;
        bool agentEngaged = record.UsedAgent;

        if (record.TotalsByFeature is not null)
        {
            foreach (var feat in record.TotalsByFeature)
            {
                if (feat.Feature.Contains("chat", StringComparison.OrdinalIgnoreCase) && feat.UserInitiatedInteractionCount > 0)
                    chatEngaged = true;
                if (feat.Feature.Contains("agent", StringComparison.OrdinalIgnoreCase) && feat.UserInitiatedInteractionCount > 0)
                    agentEngaged = true;
            }
        }

        // Also check IDE-level features (older new-format variant)
        if (record.TotalsByIde is not null)
        {
            foreach (var ide in record.TotalsByIde)
            {
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
            }
        }

        var usage = new DailyUsage
        {
            UserLogin = record.UserLogin,
            Date = date,
            IsActive = record.IsActive,
            IsEngaged = totalAcceptances > 0 || record.UserInitiatedInteractionCount > 0,
            CompletionsSuggestions = totalSuggestions,
            CompletionsAcceptances = totalAcceptances,
            CompletionsLinesSuggested = totalLinesSuggested > 0 ? totalLinesSuggested : record.LocSuggestedToAddSum,
            CompletionsLinesAccepted = totalLinesAccepted > 0 ? totalLinesAccepted : record.LocAddedSum,
            ChatEngaged = chatEngaged,
            AgentEngaged = agentEngaged,
            CliEngaged = record.UsedCli || (record.TotalsByCli?.IsEngagedUser ?? false),
            PrimaryEditor = primaryEditor,
            PrimaryLanguage = primaryLanguage,

            // New fields
            LocSuggestedToAdd = record.LocSuggestedToAddSum,
            LocSuggestedToDelete = record.LocSuggestedToDeleteSum,
            LocAdded = record.LocAddedSum,
            LocDeleted = record.LocDeletedSum,
            InteractionCount = record.UserInitiatedInteractionCount,
            CodeGenerationCount = record.CodeGenerationActivityCount,
            CodeAcceptanceCount = record.CodeAcceptanceActivityCount,
            UsedChat = record.UsedChat,
            UsedAgent = record.UsedAgent,
            UsedCli = record.UsedCli,
            ChatAgentModeCount = record.ChatPanelAgentMode ?? 0,
            ChatAskModeCount = record.ChatPanelAskMode ?? 0,
            ChatEditModeCount = record.ChatPanelEditMode ?? 0,
        };

        PopulateCliFields(usage, record);

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

            // Legacy format doesn't have these directly, use what's available
            LocSuggestedToAdd = record.LocSuggestedToAddSum > 0 ? record.LocSuggestedToAddSum : totalLinesSuggested,
            LocSuggestedToDelete = record.LocSuggestedToDeleteSum,
            LocAdded = record.LocAddedSum > 0 ? record.LocAddedSum : totalLinesAccepted,
            LocDeleted = record.LocDeletedSum,
            InteractionCount = record.UserInitiatedInteractionCount,
            CodeGenerationCount = record.CodeGenerationActivityCount > 0 ? record.CodeGenerationActivityCount : totalSuggestions,
            CodeAcceptanceCount = record.CodeAcceptanceActivityCount > 0 ? record.CodeAcceptanceActivityCount : totalAcceptances,
            UsedChat = record.CopilotIdeChat?.IsEngagedUser ?? false,
            UsedAgent = record.CopilotIdeAgent?.IsEngagedUser ?? false,
            UsedCli = record.TotalsByCli?.IsEngagedUser ?? false,
            ChatAgentModeCount = record.ChatPanelAgentMode ?? 0,
            ChatAskModeCount = record.ChatPanelAskMode ?? 0,
            ChatEditModeCount = record.ChatPanelEditMode ?? 0,
        };

        PopulateCliFields(usage, record);

        return (usage, details);
    }

    private static void PopulateCliFields(DailyUsage usage, UserMetricsRecord record)
    {
        if (record.TotalsByCli is { } cli)
        {
            usage.CliSessionCount = cli.SessionCount;
            usage.CliRequestCount = cli.RequestCount;
            if (cli.TokenUsage is { } tokens)
            {
                usage.CliPromptTokens = tokens.PromptTokensSum;
                usage.CliOutputTokens = tokens.OutputTokensSum;
            }
        }
    }
}
