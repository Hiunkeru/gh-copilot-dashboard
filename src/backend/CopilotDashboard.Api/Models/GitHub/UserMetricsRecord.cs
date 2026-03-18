using System.Text.Json.Serialization;

namespace CopilotDashboard.Api.Models.GitHub;

/// <summary>
/// Matches the REAL format from GitHub Copilot Usage Metrics API (NDJSON).
/// Fields based on actual API response, not documentation.
/// </summary>
public class UserMetricsRecord
{
    // Date field — the API uses "day", not "date"
    [JsonPropertyName("day")]
    public string? Day { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("user_login")]
    public string UserLogin { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName("enterprise_id")]
    public string? EnterpriseId { get; set; }

    // Top-level activity counters
    [JsonPropertyName("user_initiated_interaction_count")]
    public int UserInitiatedInteractionCount { get; set; }

    [JsonPropertyName("code_generation_activity_count")]
    public int CodeGenerationActivityCount { get; set; }

    [JsonPropertyName("code_acceptance_activity_count")]
    public int CodeAcceptanceActivityCount { get; set; }

    // Real format: totals_by_ide
    [JsonPropertyName("totals_by_ide")]
    public List<IdeTotals>? TotalsByIde { get; set; }

    // Legacy format fields (in case some responses still use them)
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

    [JsonPropertyName("totals_by_cli")]
    public LegacyFeatureSection? TotalsByCli { get; set; }

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

    [JsonPropertyName("totals_by_language")]
    public List<LanguageTotals>? TotalsByLanguage { get; set; }

    [JsonPropertyName("totals_by_feature")]
    public List<FeatureTotals>? TotalsByFeature { get; set; }
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

public class FeatureTotals
{
    [JsonPropertyName("feature")]
    public string Feature { get; set; } = string.Empty; // "code_completions", "chat", "agent", etc.

    [JsonPropertyName("user_initiated_interaction_count")]
    public int UserInitiatedInteractionCount { get; set; }

    [JsonPropertyName("code_generation_activity_count")]
    public int CodeGenerationActivityCount { get; set; }

    [JsonPropertyName("code_acceptance_activity_count")]
    public int CodeAcceptanceActivityCount { get; set; }
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
