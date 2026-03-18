using CopilotDashboard.Api.Configuration;
using CopilotDashboard.Api.Data;
using CopilotDashboard.Api.Models.Domain;
using CopilotDashboard.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CopilotDashboard.Api.BackgroundServices;

public class DailyMetricsSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SyncOptions _syncOptions;
    private readonly ILogger<DailyMetricsSyncService> _logger;

    public DailyMetricsSyncService(
        IServiceScopeFactory scopeFactory,
        IOptions<SyncOptions> syncOptions,
        ILogger<DailyMetricsSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _syncOptions = syncOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_syncOptions.Enabled)
        {
            _logger.LogInformation("Sync service is disabled");
            return;
        }

        // Run initial sync on startup
        await RunSyncAsync(stoppingToken);

        // Then run daily at configured time
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(6); // 6 AM UTC next day
            if (now.Hour < 6)
                nextRun = now.Date.AddHours(6); // 6 AM UTC today

            var delay = nextRun - now;
            _logger.LogInformation("Next sync scheduled at {NextRun} (in {Delay})", nextRun, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
                await RunSyncAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public async Task RunSyncAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting Copilot metrics sync...");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var githubService = scope.ServiceProvider.GetRequiredService<IGitHubCopilotService>();
            var flattener = scope.ServiceProvider.GetRequiredService<IMetricsFlattener>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1. Sync seats
            await SyncSeatsAsync(githubService, db, ct);

            // 2. Fetch and process user metrics
            var report = await githubService.GetUserMetrics28DayAsync(ct);
            var usages = new List<DailyUsage>();
            var details = new List<DailyUsageDetail>();

            await foreach (var record in githubService.StreamNdjsonRecordsAsync(report.DownloadLinks, ct))
            {
                var (usage, recordDetails) = flattener.Flatten(record);
                usages.Add(usage);
                details.AddRange(recordDetails);
            }

            _logger.LogInformation("Parsed {UsageCount} usage records and {DetailCount} detail records",
                usages.Count, details.Count);

            // 3. Upsert daily_usage records
            await UpsertDailyUsagesAsync(db, usages, ct);

            // 4. Upsert daily_usage_detail records
            await UpsertDailyUsageDetailsAsync(db, details, ct);

            // 5. Recalculate daily aggregates
            await RecalculateAggregatesAsync(db, usages, ct);

            _logger.LogInformation("Sync completed successfully. Processed {Count} user-day records", usages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Copilot metrics sync");
        }
    }

    private async Task SyncSeatsAsync(IGitHubCopilotService githubService, AppDbContext db, CancellationToken ct)
    {
        var seats = await githubService.GetAllSeatsAsync(ct);

        // Mark all users as not having seat, then re-mark those that do
        await db.Users.ExecuteUpdateAsync(s => s.SetProperty(u => u.HasSeat, false), ct);

        foreach (var seat in seats)
        {
            if (seat.Assignee is null) continue;

            var login = seat.Assignee.Login;
            var existing = await db.Users.FindAsync([login], ct);

            if (existing is null)
            {
                db.Users.Add(new User
                {
                    UserLogin = login,
                    DisplayName = seat.Assignee.Name,
                    Team = seat.AssigningTeam?.Slug,
                    Organization = seat.Organization,
                    HasSeat = true,
                    SeatAssignedDate = DateOnly.TryParse(seat.CreatedAt, out var d) ? d : null,
                    LastSyncedAt = DateTime.UtcNow,
                });
            }
            else
            {
                existing.HasSeat = true;
                existing.DisplayName = seat.Assignee.Name ?? existing.DisplayName;
                existing.Team = seat.AssigningTeam?.Slug ?? existing.Team;
                existing.Organization = seat.Organization ?? existing.Organization;
                existing.LastSyncedAt = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Synced {Count} seats", seats.Count);
    }

    private async Task UpsertDailyUsagesAsync(AppDbContext db, List<DailyUsage> usages, CancellationToken ct)
    {
        var dates = usages.Select(u => u.Date).Distinct().ToList();
        var userLogins = usages.Select(u => u.UserLogin).Distinct().ToList();

        // Ensure all users exist
        var existingUsers = await db.Users.Where(u => userLogins.Contains(u.UserLogin))
            .Select(u => u.UserLogin).ToHashSetAsync(ct);

        foreach (var login in userLogins.Where(l => !existingUsers.Contains(l)))
        {
            db.Users.Add(new User
            {
                UserLogin = login,
                HasSeat = false,
                LastSyncedAt = DateTime.UtcNow,
            });
        }
        await db.SaveChangesAsync(ct);

        // Delete existing records for the date range then bulk insert
        await db.DailyUsages
            .Where(d => dates.Contains(d.Date) && userLogins.Contains(d.UserLogin))
            .ExecuteDeleteAsync(ct);

        db.DailyUsages.AddRange(usages);
        await db.SaveChangesAsync(ct);
    }

    private async Task UpsertDailyUsageDetailsAsync(AppDbContext db, List<DailyUsageDetail> details, CancellationToken ct)
    {
        if (details.Count == 0) return;

        var dates = details.Select(d => d.Date).Distinct().ToList();
        var userLogins = details.Select(d => d.UserLogin).Distinct().ToList();

        await db.DailyUsageDetails
            .Where(d => dates.Contains(d.Date) && userLogins.Contains(d.UserLogin))
            .ExecuteDeleteAsync(ct);

        db.DailyUsageDetails.AddRange(details);
        await db.SaveChangesAsync(ct);
    }

    private async Task RecalculateAggregatesAsync(AppDbContext db, List<DailyUsage> usages, CancellationToken ct)
    {
        var byDate = usages.GroupBy(u => u.Date);

        foreach (var group in byDate)
        {
            var totalSuggestions = group.Sum(u => u.CompletionsSuggestions);
            var totalAcceptances = group.Sum(u => u.CompletionsAcceptances);
            var rate = totalSuggestions > 0 ? (decimal)totalAcceptances / totalSuggestions : 0;

            var aggregate = await db.DailyAggregates.FindAsync([group.Key], ct);

            if (aggregate is null)
            {
                db.DailyAggregates.Add(new DailyAggregate
                {
                    Date = group.Key,
                    TotalActiveUsers = group.Count(u => u.IsActive),
                    TotalEngagedUsers = group.Count(u => u.IsEngaged),
                    TotalSuggestions = totalSuggestions,
                    TotalAcceptances = totalAcceptances,
                    AcceptanceRate = rate,
                });
            }
            else
            {
                aggregate.TotalActiveUsers = group.Count(u => u.IsActive);
                aggregate.TotalEngagedUsers = group.Count(u => u.IsEngaged);
                aggregate.TotalSuggestions = totalSuggestions;
                aggregate.TotalAcceptances = totalAcceptances;
                aggregate.AcceptanceRate = rate;
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
