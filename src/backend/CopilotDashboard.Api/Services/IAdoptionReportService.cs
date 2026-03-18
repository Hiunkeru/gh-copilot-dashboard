using CopilotDashboard.Api.Models.Dto;

namespace CopilotDashboard.Api.Services;

public interface IAdoptionReportService
{
    Task<AdoptionReportDto> GenerateReportAsync(CancellationToken ct = default);
}
