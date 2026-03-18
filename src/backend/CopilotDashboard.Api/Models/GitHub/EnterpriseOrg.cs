using System.Text.Json.Serialization;

namespace CopilotDashboard.Api.Models.GitHub;

public class EnterpriseOrg
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;
}
