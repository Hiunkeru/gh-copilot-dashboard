using System.Text.Json;
using System.Text.Json.Serialization;

namespace CopilotDashboard.Api.Models.GitHub;

/// <summary>
/// Matches ALL fields from the GitHub Copilot Usage Metrics API (NDJSON).
/// See: https://docs.github.com/en/rest/copilot/copilot-usage
/// </summary>
public class UserMetricsRecord
{
    [JsonPropertyName("report_start_day")]
    public string? ReportStartDay { get; set; }

    [JsonPropertyName("report_end_day")]
    public string? ReportEndDay { get; set; }

    [JsonPropertyName("day")]
    public string? Day { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("enterprise_id")]
    public string? EnterpriseId { get; set; }

    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName("user_login")]
    public string UserLogin { get; set; } = string.Empty;

    // Top-level activity counters
    [JsonPropertyName("user_initiated_interaction_count")]
    public int UserInitiatedInteractionCount { get; set; }

    [JsonPropertyName("code_generation_activity_count")]
    public int CodeGenerationActivityCount { get; set; }

    [JsonPropertyName("code_acceptance_activity_count")]
    public int CodeAcceptanceActivityCount { get; set; }

    // LOC totals at the top level
    [JsonPropertyName("loc_suggested_to_add_sum")]
    public int LocSuggestedToAddSum { get; set; }

    [JsonPropertyName("loc_suggested_to_delete_sum")]
    public int LocSuggestedToDeleteSum { get; set; }

    [JsonPropertyName("loc_added_sum")]
    public int LocAddedSum { get; set; }

    [JsonPropertyName("loc_deleted_sum")]
    public int LocDeletedSum { get; set; }

    // Feature flags
    [JsonPropertyName("used_agent")]
    public bool UsedAgent { get; set; }

    [JsonPropertyName("used_chat")]
    public bool UsedChat { get; set; }

    [JsonPropertyName("used_cli")]
    public bool UsedCli { get; set; }

    // Chat mode counts
    [JsonPropertyName("chat_panel_agent_mode")]
    public int? ChatPanelAgentMode { get; set; }

    [JsonPropertyName("chat_panel_ask_mode")]
    public int? ChatPanelAskMode { get; set; }

    [JsonPropertyName("chat_panel_edit_mode")]
    public int? ChatPanelEditMode { get; set; }

    [JsonPropertyName("chat_panel_custom_mode")]
    public int? ChatPanelCustomMode { get; set; }

    [JsonPropertyName("chat_panel_unknown_mode")]
    public int? ChatPanelUnknownMode { get; set; }

    [JsonPropertyName("daily_active_cli_users")]
    public int? DailyActiveCliUsers { get; set; }

    [JsonPropertyName("agent_edit")]
    public int? AgentEdit { get; set; }

    // Breakdowns
    [JsonPropertyName("totals_by_ide")]
    public List<IdeTotals>? TotalsByIde { get; set; }

    [JsonPropertyName("totals_by_feature")]
    public List<FeatureTotals>? TotalsByFeature { get; set; }

    [JsonPropertyName("totals_by_language_feature")]
    public List<LanguageFeatureTotals>? TotalsByLanguageFeature { get; set; }

    [JsonPropertyName("totals_by_language_model")]
    public List<LanguageModelTotals>? TotalsByLanguageModel { get; set; }

    [JsonPropertyName("totals_by_model_feature")]
    public List<ModelFeatureTotals>? TotalsByModelFeature { get; set; }

    [JsonPropertyName("totals_by_cli")]
    public CliTotals? TotalsByCli { get; set; }

    [JsonPropertyName("pull_requests")]
    public PullRequestTotals? PullRequests { get; set; }

    // Legacy format fields (kept for backward compatibility with older API responses)
    [JsonPropertyName("is_active_user")]
    public bool? IsActiveUser { get; set; }

    [JsonPropertyName("is_engaged_user")]
    public bool? IsEngagedUser { get; set; }

    [JsonPropertyName("copilot_ide_code_completions")]
    public LegacyCompletionsSection? CopilotIdeCodeCompletions { get; set; }

    [JsonPropertyName("copilot_ide_chat")]
    public LegacyChatSection? CopilotIdeChat { get; set; }

    [JsonPropertyName("copilot_ide_agent")]
    public LegacyFeatureSection? CopilotIdeAgent { get; set; }

    /// <summary>
    /// Returns the effective date string from either "day" or "date" field.
    /// </summary>
    public string? EffectiveDate => !string.IsNullOrEmpty(Day) ? Day : Date;

    /// <summary>
    /// Whether this user was active — derived from activity counts or legacy field.
    /// </summary>
    public bool IsActive => IsActiveUser == true
        || CodeGenerationActivityCount > 0
        || CodeAcceptanceActivityCount > 0
        || UserInitiatedInteractionCount > 0;
}

// ---- Real API format classes ----

public class IdeTotals
{
    [JsonPropertyName("ide")]
    public string Ide { get; set; } = string.Empty;

    [JsonPropertyName("user_initiated_interaction_count")]
    public int UserInitiatedInteractionCount { get; set; }

    [JsonPropertyName("code_generation_activity_count")]
    public int CodeGenerationActivityCount { get; set; }

    [JsonPropertyName("code_acceptance_activity_count")]
    public int CodeAcceptanceActivityCount { get; set; }

    [JsonPropertyName("loc_suggested_to_add_sum")]
    public int LocSuggestedToAddSum { get; set; }

    [JsonPropertyName("loc_suggested_to_delete_sum")]
    public int LocSuggestedToDeleteSum { get; set; }

    [JsonPropertyName("loc_added_sum")]
    public int LocAddedSum { get; set; }

    [JsonPropertyName("loc_deleted_sum")]
    public int LocDeletedSum { get; set; }

    [JsonPropertyName("last_known_plugin_version")]
    public VersionInfo? LastKnownPluginVersion { get; set; }

    [JsonPropertyName("last_known_ide_version")]
    public IdeVersionInfo? LastKnownIdeVersion { get; set; }

    // Legacy nested breakdowns (older format within totals_by_ide)
    [JsonPropertyName("totals_by_language")]
    public List<LanguageTotals>? TotalsByLanguage { get; set; }

    [JsonPropertyName("totals_by_feature")]
    public List<FeatureTotals>? TotalsByFeature { get; set; }
}

public class VersionInfo
{
    [JsonPropertyName("sampled_at")]
    public string? SampledAt { get; set; }

    [JsonPropertyName("plugin")]
    public string? Plugin { get; set; }

    [JsonPropertyName("plugin_version")]
    public string? PluginVersion { get; set; }
}

public class IdeVersionInfo
{
    [JsonPropertyName("sampled_at")]
    public string? SampledAt { get; set; }

    [JsonPropertyName("ide_version")]
    public string? IdeVersion { get; set; }
}

public class FeatureTotals
{
    [JsonPropertyName("feature")]
    public string Feature { get; set; } = string.Empty;

    [JsonPropertyName("user_initiated_interaction_count")]
    public int UserInitiatedInteractionCount { get; set; }

    [JsonPropertyName("code_generation_activity_count")]
    public int CodeGenerationActivityCount { get; set; }

    [JsonPropertyName("code_acceptance_activity_count")]
    public int CodeAcceptanceActivityCount { get; set; }

    [JsonPropertyName("loc_suggested_to_add_sum")]
    public int LocSuggestedToAddSum { get; set; }

    [JsonPropertyName("loc_suggested_to_delete_sum")]
    public int LocSuggestedToDeleteSum { get; set; }

    [JsonPropertyName("loc_added_sum")]
    public int LocAddedSum { get; set; }

    [JsonPropertyName("loc_deleted_sum")]
    public int LocDeletedSum { get; set; }
}

public class LanguageFeatureTotals
{
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("feature")]
    public string Feature { get; set; } = string.Empty;

    [JsonPropertyName("code_generation_activity_count")]
    public int CodeGenerationActivityCount { get; set; }

    [JsonPropertyName("code_acceptance_activity_count")]
    public int CodeAcceptanceActivityCount { get; set; }

    [JsonPropertyName("loc_suggested_to_add_sum")]
    public int LocSuggestedToAddSum { get; set; }

    [JsonPropertyName("loc_suggested_to_delete_sum")]
    public int LocSuggestedToDeleteSum { get; set; }

    [JsonPropertyName("loc_added_sum")]
    public int LocAddedSum { get; set; }

    [JsonPropertyName("loc_deleted_sum")]
    public int LocDeletedSum { get; set; }
}

public class LanguageModelTotals
{
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("code_generation_activity_count")]
    public int CodeGenerationActivityCount { get; set; }

    [JsonPropertyName("code_acceptance_activity_count")]
    public int CodeAcceptanceActivityCount { get; set; }
}

public class ModelFeatureTotals
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("feature")]
    public string Feature { get; set; } = string.Empty;

    [JsonPropertyName("code_generation_activity_count")]
    public int CodeGenerationActivityCount { get; set; }

    [JsonPropertyName("code_acceptance_activity_count")]
    public int CodeAcceptanceActivityCount { get; set; }
}

public class LanguageTotals
{
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("code_generation_activity_count")]
    public int CodeGenerationActivityCount { get; set; }

    [JsonPropertyName("code_acceptance_activity_count")]
    public int CodeAcceptanceActivityCount { get; set; }

    [JsonPropertyName("code_lines_generated_count")]
    public int CodeLinesGeneratedCount { get; set; }

    [JsonPropertyName("code_lines_accepted_count")]
    public int CodeLinesAcceptedCount { get; set; }
}

public class CliTotals
{
    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }

    [JsonPropertyName("session_count")]
    public int SessionCount { get; set; }

    [JsonPropertyName("request_count")]
    public int RequestCount { get; set; }

    [JsonPropertyName("prompt_count")]
    public int PromptCount { get; set; }

    [JsonPropertyName("last_known_cli_version")]
    public JsonElement? LastKnownCliVersion { get; set; }

    [JsonPropertyName("token_usage")]
    public CliTokenUsage? TokenUsage { get; set; }
}

public class CliTokenUsage
{
    [JsonPropertyName("output_tokens_sum")]
    public int OutputTokensSum { get; set; }

    [JsonPropertyName("prompt_tokens_sum")]
    public int PromptTokensSum { get; set; }

    [JsonPropertyName("avg_tokens_per_request")]
    public decimal AvgTokensPerRequest { get; set; }
}

public class PullRequestTotals
{
    [JsonPropertyName("total_created")]
    public int TotalCreated { get; set; }

    [JsonPropertyName("total_reviewed")]
    public int TotalReviewed { get; set; }

    [JsonPropertyName("total_merged")]
    public int TotalMerged { get; set; }

    [JsonPropertyName("median_minutes_to_merge")]
    public decimal? MedianMinutesToMerge { get; set; }

    [JsonPropertyName("total_suggestions")]
    public int TotalSuggestions { get; set; }

    [JsonPropertyName("total_applied_suggestions")]
    public int TotalAppliedSuggestions { get; set; }

    [JsonPropertyName("total_created_by_copilot")]
    public int TotalCreatedByCopilot { get; set; }

    [JsonPropertyName("total_reviewed_by_copilot")]
    public int TotalReviewedByCopilot { get; set; }

    [JsonPropertyName("total_merged_created_by_copilot")]
    public int TotalMergedCreatedByCopilot { get; set; }

    [JsonPropertyName("median_minutes_to_merge_copilot_authored")]
    public decimal? MedianMinutesToMergeCopilotAuthored { get; set; }

    [JsonPropertyName("total_copilot_suggestions")]
    public int TotalCopilotSuggestions { get; set; }

    [JsonPropertyName("total_copilot_applied_suggestions")]
    public int TotalCopilotAppliedSuggestions { get; set; }
}

// ---- Legacy format classes (kept for backward compatibility) ----

public class LegacyCompletionsSection
{
    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }

    [JsonPropertyName("editors")]
    public List<LegacyEditorEntry>? Editors { get; set; }
}

public class LegacyEditorEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }

    [JsonPropertyName("models")]
    public List<LegacyModelEntry>? Models { get; set; }
}

public class LegacyModelEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("languages")]
    public List<LegacyLanguageEntry>? Languages { get; set; }
}

public class LegacyLanguageEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("total_code_suggestions")]
    public int TotalCodeSuggestions { get; set; }

    [JsonPropertyName("total_code_acceptances")]
    public int TotalCodeAcceptances { get; set; }

    [JsonPropertyName("total_code_lines_suggested")]
    public int TotalCodeLinesSuggested { get; set; }

    [JsonPropertyName("total_code_lines_accepted")]
    public int TotalCodeLinesAccepted { get; set; }
}

public class LegacyChatSection
{
    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }
}

public class LegacyFeatureSection
{
    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }
}
