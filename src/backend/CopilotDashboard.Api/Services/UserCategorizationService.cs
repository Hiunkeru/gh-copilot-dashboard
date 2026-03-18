namespace CopilotDashboard.Api.Services;

public enum UserCategory
{
    PowerUser,
    Occasional,
    Inactive,
    NeverUsed
}

public interface IUserCategorizationService
{
    UserCategory Categorize(int activeDays, decimal acceptanceRate, bool hasSeat);
}

public class UserCategorizationService : IUserCategorizationService
{
    public UserCategory Categorize(int activeDays, decimal acceptanceRate, bool hasSeat)
    {
        if (!hasSeat) return UserCategory.NeverUsed;
        if (activeDays == 0) return UserCategory.NeverUsed;
        if (activeDays >= 16 && acceptanceRate >= 0.5m) return UserCategory.PowerUser;
        if (activeDays >= 4) return UserCategory.Occasional;
        return UserCategory.Inactive;
    }
}
