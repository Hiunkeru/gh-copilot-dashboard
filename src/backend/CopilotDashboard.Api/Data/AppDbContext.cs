using CopilotDashboard.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CopilotDashboard.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<DailyUsage> DailyUsages => Set<DailyUsage>();
    public DbSet<DailyAggregate> DailyAggregates => Set<DailyAggregate>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<DailyUsageDetail> DailyUsageDetails => Set<DailyUsageDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserLogin);
            entity.Property(e => e.UserLogin).HasColumnName("user_login").HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(200);
            entity.Property(e => e.Team).HasColumnName("team").HasMaxLength(100);
            entity.Property(e => e.Organization).HasColumnName("organization").HasMaxLength(100);
            entity.Property(e => e.HasSeat).HasColumnName("has_seat");
            entity.Property(e => e.SeatAssignedDate).HasColumnName("seat_assigned_date");
            entity.Property(e => e.LastSyncedAt).HasColumnName("last_synced_at");
        });

        modelBuilder.Entity<DailyUsage>(entity =>
        {
            entity.ToTable("daily_usage");
            entity.HasKey(e => new { e.UserLogin, e.Date });
            entity.Property(e => e.UserLogin).HasColumnName("user_login").HasMaxLength(100);
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.IsEngaged).HasColumnName("is_engaged");
            entity.Property(e => e.CompletionsSuggestions).HasColumnName("completions_suggestions");
            entity.Property(e => e.CompletionsAcceptances).HasColumnName("completions_acceptances");
            entity.Property(e => e.CompletionsLinesSuggested).HasColumnName("completions_lines_suggested");
            entity.Property(e => e.CompletionsLinesAccepted).HasColumnName("completions_lines_accepted");
            entity.Property(e => e.ChatEngaged).HasColumnName("chat_engaged");
            entity.Property(e => e.AgentEngaged).HasColumnName("agent_engaged");
            entity.Property(e => e.CliEngaged).HasColumnName("cli_engaged");
            entity.Property(e => e.PrimaryEditor).HasColumnName("primary_editor").HasMaxLength(50);
            entity.Property(e => e.PrimaryLanguage).HasColumnName("primary_language").HasMaxLength(50);

            // New metric fields
            entity.Property(e => e.LocSuggestedToAdd).HasColumnName("loc_suggested_to_add");
            entity.Property(e => e.LocSuggestedToDelete).HasColumnName("loc_suggested_to_delete");
            entity.Property(e => e.LocAdded).HasColumnName("loc_added");
            entity.Property(e => e.LocDeleted).HasColumnName("loc_deleted");
            entity.Property(e => e.InteractionCount).HasColumnName("interaction_count");
            entity.Property(e => e.CodeGenerationCount).HasColumnName("code_generation_count");
            entity.Property(e => e.CodeAcceptanceCount).HasColumnName("code_acceptance_count");
            entity.Property(e => e.UsedChat).HasColumnName("used_chat");
            entity.Property(e => e.UsedAgent).HasColumnName("used_agent");
            entity.Property(e => e.UsedCli).HasColumnName("used_cli");
            entity.Property(e => e.ChatAgentModeCount).HasColumnName("chat_agent_mode_count");
            entity.Property(e => e.ChatAskModeCount).HasColumnName("chat_ask_mode_count");
            entity.Property(e => e.ChatEditModeCount).HasColumnName("chat_edit_mode_count");
            entity.Property(e => e.CliSessionCount).HasColumnName("cli_session_count");
            entity.Property(e => e.CliRequestCount).HasColumnName("cli_request_count");
            entity.Property(e => e.CliPromptTokens).HasColumnName("cli_prompt_tokens");
            entity.Property(e => e.CliOutputTokens).HasColumnName("cli_output_tokens");

            entity.HasOne(e => e.User)
                .WithMany(u => u.DailyUsages)
                .HasForeignKey(e => e.UserLogin);

            entity.HasIndex(e => e.Date);
        });

        modelBuilder.Entity<DailyAggregate>(entity =>
        {
            entity.ToTable("daily_aggregate");
            entity.HasKey(e => e.Date);
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.TotalActiveUsers).HasColumnName("total_active_users");
            entity.Property(e => e.TotalEngagedUsers).HasColumnName("total_engaged_users");
            entity.Property(e => e.TotalSuggestions).HasColumnName("total_suggestions");
            entity.Property(e => e.TotalAcceptances).HasColumnName("total_acceptances");
            entity.Property(e => e.AcceptanceRate).HasColumnName("acceptance_rate").HasPrecision(5, 4);
        });

        modelBuilder.Entity<DailyUsageDetail>(entity =>
        {
            entity.ToTable("daily_usage_detail");
            entity.HasKey(e => new { e.UserLogin, e.Date, e.EditorName, e.LanguageName });
            entity.Property(e => e.UserLogin).HasColumnName("user_login").HasMaxLength(100);
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.EditorName).HasColumnName("editor_name").HasMaxLength(50);
            entity.Property(e => e.LanguageName).HasColumnName("language_name").HasMaxLength(50);
            entity.Property(e => e.Suggestions).HasColumnName("suggestions");
            entity.Property(e => e.Acceptances).HasColumnName("acceptances");
            entity.Property(e => e.LinesSuggested).HasColumnName("lines_suggested");
            entity.Property(e => e.LinesAccepted).HasColumnName("lines_accepted");

            entity.HasOne(e => e.User)
                .WithMany(u => u.DailyUsageDetails)
                .HasForeignKey(e => e.UserLogin);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.GeneratedAt).HasColumnName("generated_at");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start").HasMaxLength(10);
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end").HasMaxLength(10);
            entity.Property(e => e.FullReportMarkdown).HasColumnName("full_report_markdown");
            entity.Property(e => e.TotalSeats).HasColumnName("total_seats");
            entity.Property(e => e.ActiveUsers).HasColumnName("active_users");
            entity.Property(e => e.AdoptionRate).HasColumnName("adoption_rate").HasPrecision(5, 1);
            entity.Property(e => e.AcceptanceRate).HasColumnName("acceptance_rate").HasPrecision(5, 1);
            entity.Property(e => e.GeneratedBy).HasColumnName("generated_by").HasMaxLength(100);
        });
    }
}
