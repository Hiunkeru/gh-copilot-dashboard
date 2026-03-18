using Azure.Data.Tables;
using CopilotDashboard.Api.Models.Domain;

namespace CopilotDashboard.Api.Services;

/// <summary>
/// Azure Table Storage report store for production.
/// Table: reports, PartitionKey: "report", RowKey: reverse-tick ID for natural descending order.
/// </summary>
public class TableStorageReportStore : IReportStore
{
    private readonly TableClient _tableClient;
    private readonly ILogger<TableStorageReportStore> _logger;

    public TableStorageReportStore(TableClient tableClient, ILogger<TableStorageReportStore> logger)
    {
        _tableClient = tableClient;
        _logger = logger;
    }

    public async Task SaveAsync(Report report, CancellationToken ct = default)
    {
        // Use reverse ticks so newest reports sort first naturally
        var reverseTicks = (DateTime.MaxValue.Ticks - report.GeneratedAt.Ticks).ToString("D19");
        report.Id = (int)(report.GeneratedAt.Ticks % int.MaxValue); // Deterministic ID from timestamp

        var entity = new TableEntity("report", reverseTicks)
        {
            { "Id", report.Id },
            { "GeneratedAt", report.GeneratedAt },
            { "PeriodStart", report.PeriodStart },
            { "PeriodEnd", report.PeriodEnd },
            { "FullReportMarkdown", report.FullReportMarkdown },
            { "TotalSeats", report.TotalSeats },
            { "ActiveUsers", report.ActiveUsers },
            { "AdoptionRate", (double)report.AdoptionRate },
            { "AcceptanceRate", (double)report.AcceptanceRate },
            { "GeneratedBy", report.GeneratedBy },
        };

        await _tableClient.AddEntityAsync(entity, ct);
        _logger.LogInformation("Saved report {RowKey} to Table Storage", reverseTicks);
    }

    public async Task<List<Report>> GetAllAsync(CancellationToken ct = default)
    {
        var reports = new List<Report>();

        await foreach (var entity in _tableClient.QueryAsync<TableEntity>(
            filter: "PartitionKey eq 'report'",
            select: new[] { "Id", "GeneratedAt", "PeriodStart", "PeriodEnd", "TotalSeats", "ActiveUsers", "AdoptionRate", "AcceptanceRate", "GeneratedBy" },
            cancellationToken: ct))
        {
            reports.Add(MapToReport(entity, includeMarkdown: false));
        }

        return reports; // Already sorted by RowKey (reverse ticks = newest first)
    }

    public async Task<Report?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await foreach (var entity in _tableClient.QueryAsync<TableEntity>(
            filter: $"PartitionKey eq 'report' and Id eq {id}",
            cancellationToken: ct))
        {
            return MapToReport(entity, includeMarkdown: true);
        }

        return null;
    }

    private static Report MapToReport(TableEntity entity, bool includeMarkdown)
    {
        return new Report
        {
            Id = entity.GetInt32("Id") ?? 0,
            GeneratedAt = entity.GetDateTime("GeneratedAt") ?? DateTime.MinValue,
            PeriodStart = entity.GetString("PeriodStart") ?? "",
            PeriodEnd = entity.GetString("PeriodEnd") ?? "",
            FullReportMarkdown = includeMarkdown ? entity.GetString("FullReportMarkdown") ?? "" : "",
            TotalSeats = entity.GetInt32("TotalSeats") ?? 0,
            ActiveUsers = entity.GetInt32("ActiveUsers") ?? 0,
            AdoptionRate = (decimal)(entity.GetDouble("AdoptionRate") ?? 0),
            AcceptanceRate = (decimal)(entity.GetDouble("AcceptanceRate") ?? 0),
            GeneratedBy = entity.GetString("GeneratedBy") ?? "system",
        };
    }
}
