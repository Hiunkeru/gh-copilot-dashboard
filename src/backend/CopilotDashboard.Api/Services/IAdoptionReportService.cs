using CopilotDashboard.Api.Models.Dto;

namespace CopilotDashboard.Api.Services;

public interface IAdoptionReportService
{
    Task<AdoptionReportDto> GenerateReportAsync(CancellationToken ct = default);
    Task<List<ReportListItemDto>> GetReportsAsync(CancellationToken ct = default);
    Task<AdoptionReportDto?> GetReportByIdAsync(int id, CancellationToken ct = default);
}
