using CopilotDashboard.Api.Models.Domain;

namespace CopilotDashboard.Api.Services;

public interface IReportStore
{
    Task SaveAsync(Report report, CancellationToken ct = default);
    Task<List<Report>> GetAllAsync(CancellationToken ct = default);
    Task<Report?> GetByIdAsync(int id, CancellationToken ct = default);
}
