using CopilotDashboard.Api.Configuration;
using CopilotDashboard.Api.Data;
using CopilotDashboard.Api.Models.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CopilotDashboard.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly IUserCategorizationService _categorization;
    private readonly GitHubOptions _options;

    public DashboardService(
        AppDbContext db,
        IUserCategorizationService categorization,
        IOptions<GitHubOptions> options)
    {
        _db = db;
        _categorization = categorization;
        _options = options.Value;
    }

    public async Task<AdoptionOverviewDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var since = today.AddDays(-28);

        var totalSeats = await _db.Users.CountAsync(u => u.HasSeat, ct);
        var usageData = await _db.DailyUsages
            .Where(d => d.Date >= since)
            .ToListAsync(ct);

        var activeUserLogins = usageData.Where(d => d.IsActive).Select(d => d.UserLogin).Distinct().Count();
        var engagedUserLogins = usageData.Where(d => d.IsEngaged).Select(d => d.UserLogin).Distinct().Count();

        var latestDate = usageData.Any() ? usageData.Max(d => d.Date) : today;
        var dauData = usageData.Where(d => d.Date == latestDate);
        var dau = dauData.Count(d => d.IsActive);

        var weekAgo = latestDate.AddDays(-7);
        var wau = usageData
            .Where(d => d.Date >= weekAgo && d.IsActive)
            .Select(d => d.UserLogin)
            .Distinct()
            .Count();

        var wastedSeats = totalSeats - activeUserLogins;

        var totalSuggestions = usageData.Sum(d => d.CompletionsSuggestions);
        var totalAcceptances = usageData.Sum(d => d.CompletionsAcceptances);
        var totalLinesAccepted = usageData.Sum(d => d.CompletionsLinesAccepted);
        var acceptanceRate = totalSuggestions > 0 ? (decimal)totalAcceptances / totalSuggestions : 0;

        return new AdoptionOverviewDto
        {
            TotalSeats = totalSeats,
            ActiveUsers = activeUserLogins,
            EngagedUsers = engagedUserLogins,
            AdoptionRate = totalSeats > 0 ? (decimal)activeUserLogins / totalSeats * 100 : 0,
            DailyActiveUsers = dau,
            WeeklyActiveUsers = wau,
            WastedSeats = Math.Max(0, wastedSeats),
            TotalSuggestions = totalSuggestions,
            TotalAcceptances = totalAcceptances,
            AcceptanceRate = Math.Round(acceptanceRate * 100, 1),
            TotalLinesAccepted = totalLinesAccepted,
            DataAsOf = latestDate.ToString("yyyy-MM-dd"),
        };
    }

    public async Task<List<TrendPointDto>> GetTrendsAsync(int days = 28, CancellationToken ct = default)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

        var aggregates = await _db.DailyAggregates
            .Where(a => a.Date >= since)
            .OrderBy(a => a.Date)
            .ToListAsync(ct);

        return aggregates.Select(a => new TrendPointDto
        {
            Date = a.Date.ToString("yyyy-MM-dd"),
            ActiveUsers = a.TotalActiveUsers,
            EngagedUsers = a.TotalEngagedUsers,
            Suggestions = a.TotalSuggestions,
            Acceptances = a.TotalAcceptances,
            AcceptanceRate = Math.Round(a.AcceptanceRate * 100, 1),
        }).ToList();
    }

    public async Task<FeatureUsageDto> GetFeatureUsageAsync(int days = 28, CancellationToken ct = default)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

        var usageData = await _db.DailyUsages
            .Where(d => d.Date >= since && d.IsActive)
            .ToListAsync(ct);

        var totalUsers = usageData.Select(d => d.UserLogin).Distinct().Count();
        var completionsUsers = usageData.Where(d => d.CompletionsSuggestions > 0).Select(d => d.UserLogin).Distinct().Count();
        var chatUsers = usageData.Where(d => d.ChatEngaged).Select(d => d.UserLogin).Distinct().Count();
        var agentUsers = usageData.Where(d => d.AgentEngaged).Select(d => d.UserLogin).Distinct().Count();
        var cliUsers = usageData.Where(d => d.CliEngaged).Select(d => d.UserLogin).Distinct().Count();

        return new FeatureUsageDto
        {
            TotalUsers = totalUsers,
            CompletionsUsers = completionsUsers,
            ChatUsers = chatUsers,
            AgentUsers = agentUsers,
            CliUsers = cliUsers,
            CompletionsPercent = totalUsers > 0 ? Math.Round((decimal)completionsUsers / totalUsers * 100, 1) : 0,
            ChatPercent = totalUsers > 0 ? Math.Round((decimal)chatUsers / totalUsers * 100, 1) : 0,
            AgentPercent = totalUsers > 0 ? Math.Round((decimal)agentUsers / totalUsers * 100, 1) : 0,
            CliPercent = totalUsers > 0 ? Math.Round((decimal)cliUsers / totalUsers * 100, 1) : 0,
        };
    }

    public async Task<List<DistributionItemDto>> GetLanguageDistributionAsync(int days = 28, CancellationToken ct = default)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

        return await _db.DailyUsageDetails
            .Where(d => d.Date >= since)
            .GroupBy(d => d.LanguageName)
            .Select(g => new DistributionItemDto
            {
                Name = g.Key,
                Suggestions = g.Sum(x => x.Suggestions),
                Acceptances = g.Sum(x => x.Acceptances),
                LinesAccepted = g.Sum(x => x.LinesAccepted),
                UserCount = g.Select(x => x.UserLogin).Distinct().Count(),
            })
            .OrderByDescending(x => x.Acceptances)
            .Take(20)
            .ToListAsync(ct);
    }

    public async Task<List<DistributionItemDto>> GetEditorDistributionAsync(int days = 28, CancellationToken ct = default)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

        return await _db.DailyUsageDetails
            .Where(d => d.Date >= since)
            .GroupBy(d => d.EditorName)
            .Select(g => new DistributionItemDto
            {
                Name = g.Key,
                Suggestions = g.Sum(x => x.Suggestions),
                Acceptances = g.Sum(x => x.Acceptances),
                LinesAccepted = g.Sum(x => x.LinesAccepted),
                UserCount = g.Select(x => x.UserLogin).Distinct().Count(),
            })
            .OrderByDescending(x => x.UserCount)
            .ToListAsync(ct);
    }

    public async Task<RoiDto> GetRoiAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var since28 = today.AddDays(-28);
        var since7 = today.AddDays(-7);

        var totalSeats = await _db.Users.CountAsync(u => u.HasSeat, ct);
        var usageData = await _db.DailyUsages.Where(d => d.Date >= since28).ToListAsync(ct);

        var totalLinesAccepted = usageData.Sum(d => d.CompletionsLinesAccepted);
        var last7DaysLines = usageData.Where(d => d.Date >= since7).Sum(d => d.CompletionsLinesAccepted);
        var activeUsers = usageData.Where(d => d.IsActive).Select(d => d.UserLogin).Distinct().Count();
        var activeDays = usageData.Where(d => d.IsActive).Select(d => d.Date).Distinct().Count();

        var avgLinesPerUserPerDay = activeUsers > 0 && activeDays > 0
            ? (decimal)totalLinesAccepted / activeUsers / activeDays
            : 0;

        var costPerActiveUser = activeUsers > 0
            ? totalSeats * _options.LicenseCostPerMonth / activeUsers
            : 0;

        return new RoiDto
        {
            TotalLinesAccepted = totalLinesAccepted,
            TotalLinesAcceptedLast7Days = last7DaysLines,
            AvgLinesPerActiveUserPerDay = Math.Round(avgLinesPerUserPerDay, 1),
            CostPerActiveUser = Math.Round(costPerActiveUser, 2),
            LicenseCostPerMonth = _options.LicenseCostPerMonth,
            ActiveUsers = activeUsers,
            TotalSeats = totalSeats,
        };
    }

    public async Task<UserActivityPageDto> GetUsersAsync(
        int page = 1, int pageSize = 20, string? search = null, string? category = null,
        string? sortBy = null, bool sortDesc = true, CancellationToken ct = default)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-28);

        var usersQuery = _db.Users.Where(u => u.HasSeat);
        if (!string.IsNullOrWhiteSpace(search))
            usersQuery = usersQuery.Where(u => u.UserLogin.Contains(search) || (u.DisplayName != null && u.DisplayName.Contains(search)));

        var users = await usersQuery.ToListAsync(ct);
        var usageData = await _db.DailyUsages.Where(d => d.Date >= since).ToListAsync(ct);
        var usageByUser = usageData.GroupBy(d => d.UserLogin).ToDictionary(g => g.Key, g => g.ToList());

        var userDtos = users.Select(user =>
        {
            var userUsage = usageByUser.GetValueOrDefault(user.UserLogin, []);
            var activeDays = userUsage.Count(d => d.IsActive);
            var totalSuggestions = userUsage.Sum(d => d.CompletionsSuggestions);
            var totalAcceptances = userUsage.Sum(d => d.CompletionsAcceptances);
            var acceptanceRate = totalSuggestions > 0 ? (decimal)totalAcceptances / totalSuggestions : 0;
            var lastActivity = userUsage.Where(d => d.IsActive).MaxBy(d => d.Date)?.Date;

            var cat = _categorization.Categorize(activeDays, acceptanceRate, user.HasSeat);

            return new UserActivityDto
            {
                UserLogin = user.UserLogin,
                DisplayName = user.DisplayName,
                Team = user.Team,
                LastActivity = lastActivity?.ToString("yyyy-MM-dd"),
                ActiveDays = activeDays,
                TotalSuggestions = totalSuggestions,
                TotalAcceptances = totalAcceptances,
                AcceptanceRate = Math.Round(acceptanceRate * 100, 1),
                UsesChat = userUsage.Any(d => d.ChatEngaged),
                UsesAgent = userUsage.Any(d => d.AgentEngaged),
                UsesCli = userUsage.Any(d => d.CliEngaged),
                Category = cat.ToString(),
            };
        }).ToList();

        if (!string.IsNullOrWhiteSpace(category))
            userDtos = userDtos.Where(u => u.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

        userDtos = (sortBy?.ToLowerInvariant()) switch
        {
            "activeDays" or "activedays" => sortDesc ? userDtos.OrderByDescending(u => u.ActiveDays).ToList() : userDtos.OrderBy(u => u.ActiveDays).ToList(),
            "acceptanceRate" or "acceptancerate" => sortDesc ? userDtos.OrderByDescending(u => u.AcceptanceRate).ToList() : userDtos.OrderBy(u => u.AcceptanceRate).ToList(),
            "suggestions" => sortDesc ? userDtos.OrderByDescending(u => u.TotalSuggestions).ToList() : userDtos.OrderBy(u => u.TotalSuggestions).ToList(),
            "lastActivity" or "lastactivity" => sortDesc ? userDtos.OrderByDescending(u => u.LastActivity).ToList() : userDtos.OrderBy(u => u.LastActivity).ToList(),
            _ => userDtos.OrderByDescending(u => u.ActiveDays).ToList(),
        };

        var totalCount = userDtos.Count;
        var paged = userDtos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new UserActivityPageDto
        {
            Users = paged,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<List<TrendPointDto>> GetUserHistoryAsync(string userLogin, CancellationToken ct = default)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-28);

        var data = await _db.DailyUsages
            .Where(d => d.UserLogin == userLogin && d.Date >= since)
            .OrderBy(d => d.Date)
            .ToListAsync(ct);

        return data.Select(d => new TrendPointDto
        {
            Date = d.Date.ToString("yyyy-MM-dd"),
            ActiveUsers = d.IsActive ? 1 : 0,
            EngagedUsers = d.IsEngaged ? 1 : 0,
            Suggestions = d.CompletionsSuggestions,
            Acceptances = d.CompletionsAcceptances,
            AcceptanceRate = d.CompletionsSuggestions > 0
                ? Math.Round((decimal)d.CompletionsAcceptances / d.CompletionsSuggestions * 100, 1)
                : 0,
        }).ToList();
    }
}
