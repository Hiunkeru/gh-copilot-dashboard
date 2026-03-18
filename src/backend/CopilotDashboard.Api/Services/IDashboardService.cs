using CopilotDashboard.Api.Models.Dto;

namespace CopilotDashboard.Api.Services;

public interface IDashboardService
{
    Task<AdoptionOverviewDto> GetOverviewAsync(CancellationToken ct = default);
    Task<List<TrendPointDto>> GetTrendsAsync(int days = 28, CancellationToken ct = default);
    Task<FeatureUsageDto> GetFeatureUsageAsync(int days = 28, CancellationToken ct = default);
    Task<List<DistributionItemDto>> GetLanguageDistributionAsync(int days = 28, CancellationToken ct = default);
    Task<List<DistributionItemDto>> GetEditorDistributionAsync(int days = 28, CancellationToken ct = default);
    Task<RoiDto> GetRoiAsync(CancellationToken ct = default);
    Task<UserActivityPageDto> GetUsersAsync(int page = 1, int pageSize = 20, string? search = null, string? category = null, string? sortBy = null, bool sortDesc = true, CancellationToken ct = default);
    Task<List<TrendPointDto>> GetUserHistoryAsync(string userLogin, CancellationToken ct = default);
}
