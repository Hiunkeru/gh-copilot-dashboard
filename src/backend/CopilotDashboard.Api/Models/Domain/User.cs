namespace CopilotDashboard.Api.Models.Domain;

public class User
{
    public string UserLogin { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Team { get; set; }
    public bool HasSeat { get; set; } = true;
    public DateOnly? SeatAssignedDate { get; set; }
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DailyUsage> DailyUsages { get; set; } = new List<DailyUsage>();
    public ICollection<DailyUsageDetail> DailyUsageDetails { get; set; } = new List<DailyUsageDetail>();
}
