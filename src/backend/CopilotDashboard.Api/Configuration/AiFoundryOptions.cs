namespace CopilotDashboard.Api.Configuration;

public class AiFoundryOptions
{
    public const string SectionName = "AiFoundry";

    public string Endpoint { get; set; } = string.Empty; // https://xxx.openai.azure.com/
    public string ApiKey { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = "o4-mini";
}
