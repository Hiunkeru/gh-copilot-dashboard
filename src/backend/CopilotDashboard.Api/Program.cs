using CopilotDashboard.Api.BackgroundServices;
using CopilotDashboard.Api.Configuration;
using CopilotDashboard.Api.Data;
using CopilotDashboard.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Entra ID authentication
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");

// Configuration
builder.Services.Configure<GitHubOptions>(builder.Configuration.GetSection(GitHubOptions.SectionName));
builder.Services.Configure<SyncOptions>(builder.Configuration.GetSection(SyncOptions.SectionName));

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP Client for GitHub API with retry policy
builder.Services.AddHttpClient<IGitHubCopilotService, GitHubCopilotService>()
    .AddStandardResilienceHandler();

// Services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IMetricsFlattener, MetricsFlattener>();
builder.Services.AddSingleton<IUserCategorizationService, UserCategorizationService>();

// Background sync service
builder.Services.AddSingleton<DailyMetricsSyncService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DailyMetricsSyncService>());

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

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Response caching
builder.Services.AddResponseCaching();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
