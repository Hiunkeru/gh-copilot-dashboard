using System.Text;
using System.Text.Json;
using CopilotDashboard.Api.Configuration;
using CopilotDashboard.Api.Data;
using CopilotDashboard.Api.Models.Domain;
using CopilotDashboard.Api.Models.Dto;
using CopilotDashboard.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CopilotDashboard.Api.Services;

public class AdoptionReportService : IAdoptionReportService
{
    private readonly IDashboardService _dashboardService;
    private readonly AppDbContext _db;
    private readonly AiFoundryOptions _aiOptions;
    private readonly GitHubOptions _ghOptions;
    private readonly ILogger<AdoptionReportService> _logger;

    public AdoptionReportService(
        IDashboardService dashboardService,
        AppDbContext db,
        IOptions<AiFoundryOptions> aiOptions,
        IOptions<GitHubOptions> ghOptions,
        ILogger<AdoptionReportService> logger)
    {
        _dashboardService = dashboardService;
        _db = db;
        _aiOptions = aiOptions.Value;
        _ghOptions = ghOptions.Value;
        _logger = logger;
    }

    public async Task<AdoptionReportDto> GenerateReportAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Generating adoption report...");

        // 1. Gather all data
        var overview = await _dashboardService.GetOverviewAsync(ct);
        var trends = await _dashboardService.GetTrendsAsync(28, ct);
        var features = await _dashboardService.GetFeatureUsageAsync(28, ct);
        var languages = await _dashboardService.GetLanguageDistributionAsync(28, ct);
        var editors = await _dashboardService.GetEditorDistributionAsync(28, ct);
        var roi = await _dashboardService.GetRoiAsync(ct);
        var users = await _dashboardService.GetUsersAsync(1, 200, null, null, "activeDays", true, ct);

        // 2. Build the data context for the AI
        var dataContext = BuildDataContext(overview, trends, features, languages, editors, roi, users);

        // 3. Call Azure AI Foundry
        var reportMarkdown = await CallAiFoundryAsync(dataContext, ct);

        // 4. Parse sections from the markdown
        var report = ParseReport(reportMarkdown, overview);

        // 5. Save to database
        var entity = new Report
        {
            GeneratedAt = DateTime.UtcNow,
            PeriodStart = report.PeriodStart,
            PeriodEnd = report.PeriodEnd,
            FullReportMarkdown = reportMarkdown,
            TotalSeats = overview.TotalSeats,
            ActiveUsers = overview.ActiveUsers,
            AdoptionRate = overview.AdoptionRate,
            AcceptanceRate = overview.AcceptanceRate,
            GeneratedBy = "manual",
        };
        _db.Reports.Add(entity);
        await _db.SaveChangesAsync(ct);
        report.Id = entity.Id;

        _logger.LogInformation("Report #{Id} generated and saved ({Length} chars)", entity.Id, reportMarkdown.Length);
        return report;
    }

    public async Task<List<ReportListItemDto>> GetReportsAsync(CancellationToken ct = default)
    {
        return await _db.Reports
            .OrderByDescending(r => r.GeneratedAt)
            .Select(r => new ReportListItemDto
            {
                Id = r.Id,
                GeneratedAt = r.GeneratedAt.ToString("yyyy-MM-dd HH:mm UTC"),
                PeriodStart = r.PeriodStart,
                PeriodEnd = r.PeriodEnd,
                TotalSeats = r.TotalSeats,
                ActiveUsers = r.ActiveUsers,
                AdoptionRate = r.AdoptionRate,
                AcceptanceRate = r.AcceptanceRate,
                GeneratedBy = r.GeneratedBy,
            })
            .ToListAsync(ct);
    }

    public async Task<AdoptionReportDto?> GetReportByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Reports.FindAsync([id], ct);
        if (entity is null) return null;

        var report = ParseReport(entity.FullReportMarkdown, new AdoptionOverviewDto
        {
            DataAsOf = entity.PeriodEnd,
            TotalSeats = entity.TotalSeats,
            ActiveUsers = entity.ActiveUsers,
            AdoptionRate = entity.AdoptionRate,
            AcceptanceRate = entity.AcceptanceRate,
        });
        report.Id = entity.Id;
        report.GeneratedAt = entity.GeneratedAt.ToString("yyyy-MM-dd HH:mm UTC");
        return report;
    }

    private string BuildDataContext(
        AdoptionOverviewDto overview,
        List<TrendPointDto> trends,
        FeatureUsageDto features,
        List<DistributionItemDto> languages,
        List<DistributionItemDto> editors,
        RoiDto roi,
        UserActivityPageDto users)
    {
        var sb = new StringBuilder();

        sb.AppendLine("## ADOPTION OVERVIEW (Last 28 days)");
        sb.AppendLine($"- Total seats: {overview.TotalSeats}");
        sb.AppendLine($"- Active users: {overview.ActiveUsers} ({overview.AdoptionRate:F1}% adoption)");
        sb.AppendLine($"- Engaged users: {overview.EngagedUsers}");
        sb.AppendLine($"- Daily active users (latest): {overview.DailyActiveUsers}");
        sb.AppendLine($"- Weekly active users: {overview.WeeklyActiveUsers}");
        sb.AppendLine($"- Wasted seats (inactive): {overview.WastedSeats}");
        sb.AppendLine($"- Acceptance rate: {overview.AcceptanceRate}%");
        sb.AppendLine($"- Total suggestions: {overview.TotalSuggestions}");
        sb.AppendLine($"- Total acceptances: {overview.TotalAcceptances}");
        sb.AppendLine($"- Total lines accepted: {overview.TotalLinesAccepted}");
        sb.AppendLine($"- License cost per seat/month: ${_ghOptions.LicenseCostPerMonth}");

        sb.AppendLine("\n## ROI");
        sb.AppendLine($"- Total lines accepted (28d): {roi.TotalLinesAccepted}");
        sb.AppendLine($"- Lines accepted last 7d: {roi.TotalLinesAcceptedLast7Days}");
        sb.AppendLine($"- Avg lines/active user/day: {roi.AvgLinesPerActiveUserPerDay}");
        sb.AppendLine($"- Cost per active user: ${roi.CostPerActiveUser}");
        sb.AppendLine($"- Monthly total cost: ${roi.TotalSeats * roi.LicenseCostPerMonth}");
        sb.AppendLine($"- Wasted spend (unused seats): ${(roi.TotalSeats - roi.ActiveUsers) * roi.LicenseCostPerMonth}");

        sb.AppendLine("\n## FEATURE USAGE");
        sb.AppendLine($"- Completions: {features.CompletionsPercent}% ({features.CompletionsUsers} users)");
        sb.AppendLine($"- Chat: {features.ChatPercent}% ({features.ChatUsers} users)");
        sb.AppendLine($"- Agent: {features.AgentPercent}% ({features.AgentUsers} users)");
        sb.AppendLine($"- CLI: {features.CliPercent}% ({features.CliUsers} users)");

        sb.AppendLine("\n## DAILY TREND (last 28 days)");
        foreach (var t in trends.TakeLast(14))
        {
            sb.AppendLine($"- {t.Date}: {t.ActiveUsers} active, {t.Suggestions} suggestions, {t.Acceptances} acceptances, {t.AcceptanceRate}% rate, {t.LocAdded} LOC added");
        }

        sb.AppendLine("\n## TOP LANGUAGES");
        foreach (var l in languages.Take(10))
        {
            sb.AppendLine($"- {l.Name}: {l.Acceptances} acceptances, {l.LinesAccepted} lines, {l.UserCount} users");
        }

        sb.AppendLine("\n## EDITORS");
        foreach (var e in editors)
        {
            sb.AppendLine($"- {e.Name}: {e.UserCount} users, {e.Acceptances} acceptances");
        }

        sb.AppendLine("\n## ALL USERS (sorted by activity)");
        sb.AppendLine("| User | Org | Team | Active Days | Suggestions | Acceptances | Accept Rate | LOC Added | Interactions | Chat | Agent | CLI | Category |");
        sb.AppendLine("|------|-----|------|------------|-------------|-------------|-------------|-----------|--------------|------|-------|-----|----------|");
        foreach (var u in users.Users)
        {
            sb.AppendLine($"| {u.UserLogin} | {u.Organization ?? "-"} | {u.Team ?? "-"} | {u.ActiveDays} | {u.TotalSuggestions} | {u.TotalAcceptances} | {u.AcceptanceRate}% | {u.LocAdded} | {u.InteractionCount} | {(u.UsedChat ? "Yes" : "No")} | {(u.UsedAgent ? "Yes" : "No")} | {(u.UsedCli ? "Yes" : "No")} | {u.Category} |");
        }

        return sb.ToString();
    }

    private async Task<string> CallAiFoundryAsync(string dataContext, CancellationToken ct)
    {
        var systemPrompt = @"Eres un analista experto en adopción de herramientas de desarrollo y GitHub Copilot.
Tu objetivo es generar un informe ejecutivo de adopción de GitHub Copilot en español para la organización.

El informe debe incluir estas secciones exactas con estos títulos en markdown (## para cada sección):

## Resumen Ejecutivo
Un párrafo con el estado general de adopción, datos clave y conclusión principal.

## Análisis de Adopción
Análisis detallado del nivel de adopción: tasa de adopción, usuarios activos vs total, tendencia ascendente/descendente, comparación con benchmarks del sector (típicamente 60-80% para organizaciones maduras).

## Top Performers
Listado de los usuarios con mejor uso de Copilot. Identifica qué hacen bien (alta aceptación, uso de múltiples features, consistencia diaria). Destaca los que podrían ser referentes o mentores.

## Usuarios en Riesgo
Usuarios con licencia que no usan o usan muy poco Copilot. Clasifícalos en:
- **Never Used**: tienen licencia pero 0 actividad → acción: contactar para verificar que tienen el plugin instalado y la telemetría activa
- **Inactive**: usaron alguna vez pero ya no → acción: seguimiento para entender barreras
- **Low adoption**: usan poco y con baja aceptación → acción: formación específica

## Adopción por Feature
Análisis de qué features se usan (completions, chat, agent, CLI) y cuáles no. Recomienda cómo impulsar la adopción de features infrautilizadas.

## Tendencias
Análisis de la evolución en los últimos 28 días: ¿sube o baja la adopción? ¿hay picos o caídas? ¿correlación con algún evento?

## Recomendaciones
Acciones concretas y priorizadas para mejorar la adopción. Cada recomendación debe ser específica, medible y accionable. Formato:
1. **[Prioridad Alta/Media/Baja]** Acción concreta — Impacto esperado

## Análisis de ROI
Análisis del retorno de inversión: coste total, coste por usuario activo, gasto desperdiciado en seats no usados, líneas de código aceptadas como proxy de productividad, recomendación sobre optimización de licencias.

IMPORTANTE:
- Escribe TODO en español
- Sé directo y específico, no genérico
- Usa los datos reales proporcionados, cita números concretos
- No inventes datos que no están en el contexto
- Identifica patrones y anomalías
- Las recomendaciones deben ser accionables, no teóricas";

        // Call Azure AI Foundry directly via HTTP (to control the api-version for o4-mini)
        var url = $"{_aiOptions.Endpoint.TrimEnd('/')}/openai/deployments/{_aiOptions.DeploymentName}/chat/completions?api-version=2025-01-01-preview";

        var requestBody = new
        {
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = $"Genera el informe de adopción de GitHub Copilot basado en estos datos:\n\n{dataContext}" }
            },
            max_completion_tokens = 4000
        };

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-key", _aiOptions.ApiKey);

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(url, content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("AI Foundry returned {Status}: {Body}", response.StatusCode, responseBody);
            throw new InvalidOperationException($"AI Foundry error: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var message = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return message ?? throw new InvalidOperationException("Empty response from AI Foundry");
    }

    private AdoptionReportDto ParseReport(string markdown, AdoptionOverviewDto overview)
    {
        var report = new AdoptionReportDto
        {
            GeneratedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC"),
            PeriodEnd = overview.DataAsOf ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
            PeriodStart = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-28).ToString("yyyy-MM-dd"),
            FullReportMarkdown = markdown,
        };

        report.ExecutiveSummary = ExtractSection(markdown, "Resumen Ejecutivo");
        report.AdoptionAnalysis = ExtractSection(markdown, "Análisis de Adopción", "Analisis de Adopcion");
        report.TopPerformers = ExtractSection(markdown, "Top Performers");
        report.AtRiskUsers = ExtractSection(markdown, "Usuarios en Riesgo");
        report.FeatureAdoption = ExtractSection(markdown, "Adopción por Feature", "Adopcion por Feature");
        report.Trends = ExtractSection(markdown, "Tendencias");
        report.Recommendations = ExtractSection(markdown, "Recomendaciones");
        report.RoiAnalysis = ExtractSection(markdown, "Análisis de ROI", "Analisis de ROI");

        return report;
    }

    private static ReportSectionDto ExtractSection(string markdown, string title, string? altTitle = null)
    {
        var section = new ReportSectionDto { Title = title };

        var patterns = new List<string> { $"## {title}" };
        if (altTitle != null) patterns.Add($"## {altTitle}");

        foreach (var pattern in patterns)
        {
            var idx = markdown.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;

            var contentStart = idx + pattern.Length;
            var nextSection = markdown.IndexOf("\n## ", contentStart, StringComparison.Ordinal);
            var content = nextSection > 0
                ? markdown[contentStart..nextSection]
                : markdown[contentStart..];

            section.Content = content.Trim();
            break;
        }

        return section;
    }
}
