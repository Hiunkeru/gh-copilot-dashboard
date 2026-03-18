using CopilotDashboard.Api.Models.Domain;
using CopilotDashboard.Api.Models.GitHub;

namespace CopilotDashboard.Api.Services;

public interface IMetricsFlattener
{
    (DailyUsage Usage, List<DailyUsageDetail> Details)? Flatten(UserMetricsRecord record);
}
