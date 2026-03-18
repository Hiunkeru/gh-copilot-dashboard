using CopilotDashboard.Api.BackgroundServices;
using CopilotDashboard.Api.Configuration;
using CopilotDashboard.Api.Data;
using CopilotDashboard.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);
var isDev = builder.Environment.IsDevelopment();

// Check if real GitHub token is configured
var githubToken = builder.Configuration.GetSection("GitHub:Token").Value;
var useRealData = !string.IsNullOrWhiteSpace(githubToken) && githubToken != "<YOUR_GITHUB_PAT>";

// Authentication: Entra ID in production, anonymous in dev
if (!isDev)
{
    builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");
}
else
{
    builder.Services.AddAuthentication()
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthHandler>(
            "DevScheme", _ => { });
    builder.Services.AddAuthorization();
}

// Configuration
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection(GitHubOptions.SectionName));
builder.Services.Configure<SyncOptions>(builder.Configuration.GetSection(SyncOptions.SectionName));

// Database: InMemory for dev, SQL Server for production
if (isDev)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("CopilotDashboardDev"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// HTTP Client for GitHub API with retry policy
builder.Services.AddHttpClient<IGitHubCopilotService, GitHubCopilotService>()
    .AddStandardResilienceHandler();

// Services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IMetricsFlattener, MetricsFlattener>();
builder.Services.AddSingleton<IUserCategorizationService, UserCategorizationService>();

// Background sync service — always register it so SyncController can use it
builder.Services.AddSingleton<DailyMetricsSyncService>();
if (!isDev)
{
    builder.Services.AddHostedService(sp => sp.GetRequiredService<DailyMetricsSyncService>());
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Application Insights (only in production)
if (!isDev)
{
    builder.Services.AddApplicationInsightsTelemetry();
}

builder.Services.AddResponseCaching();

var app = builder.Build();

// Startup: seed mock data OR sync real data
if (isDev)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (useRealData)
    {
        app.Logger.LogInformation("GitHub token detected — syncing real data from GitHub API...");
        var syncService = app.Services.GetRequiredService<DailyMetricsSyncService>();
        await syncService.RunSyncAsync(CancellationToken.None);
        app.Logger.LogInformation("Real data sync completed");
    }
    else
    {
        DevDataSeeder.Seed(db);
        app.Logger.LogInformation("No GitHub token configured — using sample data. Set GitHub:Token in appsettings.Development.json to use real data.");
    }

    app.MapOpenApi();
}

app.UseCors();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
