using System.Text.Json.Serialization;

namespace CopilotDashboard.Api.Models.GitHub;

public class SeatsResponse
{
    [JsonPropertyName("total_seats")]
    public int TotalSeats { get; set; }

    [JsonPropertyName("seats")]
    public List<SeatInfo> Seats { get; set; } = new();
}

public class SeatInfo
{
    [JsonPropertyName("assignee")]
    public SeatAssignee? Assignee { get; set; }

    [JsonPropertyName("assigning_team")]
    public SeatTeam? AssigningTeam { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("last_activity_at")]
    public string? LastActivityAt { get; set; }

    [JsonPropertyName("last_activity_editor")]
    public string? LastActivityEditor { get; set; }
}

public class SeatAssignee
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class SeatTeam
{
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
