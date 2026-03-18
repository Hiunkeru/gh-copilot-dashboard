using System.Text.Json.Serialization;

namespace CopilotDashboard.Api.Models.GitHub;

public class UserMetricsRecord
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("user_login")]
    public string UserLogin { get; set; } = string.Empty;

    [JsonPropertyName("is_active_user")]
    public bool IsActiveUser { get; set; }

    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }

    [JsonPropertyName("copilot_ide_code_completions")]
    public CompletionsSection? CopilotIdeCodeCompletions { get; set; }

    [JsonPropertyName("copilot_ide_chat")]
    public ChatSection? CopilotIdeChat { get; set; }

    [JsonPropertyName("copilot_ide_agent")]
    public AgentSection? CopilotIdeAgent { get; set; }

    [JsonPropertyName("totals_by_cli")]
    public CliSection? TotalsByCli { get; set; }
}

public class CompletionsSection
{
    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }

    [JsonPropertyName("editors")]
    public List<EditorEntry>? Editors { get; set; }
}

public class EditorEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }

    [JsonPropertyName("models")]
    public List<ModelEntry>? Models { get; set; }
}

public class ModelEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_custom_model")]
    public bool IsCustomModel { get; set; }

    [JsonPropertyName("languages")]
    public List<LanguageEntry>? Languages { get; set; }
}

public class LanguageEntry
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

public class ChatSection
{
    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }

    [JsonPropertyName("editors")]
    public List<ChatEditorEntry>? Editors { get; set; }
}

public class ChatEditorEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }

    [JsonPropertyName("models")]
    public List<ChatModelEntry>? Models { get; set; }
}

public class ChatModelEntry
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("total_chats")]
    public int TotalChats { get; set; }

    [JsonPropertyName("total_chat_copy_events")]
    public int TotalChatCopyEvents { get; set; }

    [JsonPropertyName("total_chat_insertion_events")]
    public int TotalChatInsertionEvents { get; set; }
}

public class AgentSection
{
    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }
}

public class CliSection
{
    [JsonPropertyName("is_engaged_user")]
    public bool IsEngagedUser { get; set; }
}
