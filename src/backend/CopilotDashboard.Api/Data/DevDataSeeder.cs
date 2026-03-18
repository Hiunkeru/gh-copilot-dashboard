using CopilotDashboard.Api.Models.Domain;

namespace CopilotDashboard.Api.Data;

public static class DevDataSeeder
{
    private static readonly Random Rng = new(42);

    private static readonly (string Login, string Name, string Team, string Profile)[] Devs =
    [
        ("agarcia", "Ana García", "backend", "power"),
        ("jmartin", "Jorge Martín", "backend", "power"),
        ("mlopez", "María López", "frontend", "power"),
        ("crodriguez", "Carlos Rodríguez", "frontend", "power"),
        ("lsanchez", "Laura Sánchez", "data", "power"),
        ("dfernandez", "David Fernández", "backend", "power"),
        ("pnavarro", "Pablo Navarro", "devops", "power"),
        ("smoreno", "Sara Moreno", "frontend", "power"),
        ("rjimenez", "Raúl Jiménez", "backend", "occasional"),
        ("eromero", "Elena Romero", "frontend", "occasional"),
        ("atorrejon", "Andrés Torrejón", "data", "occasional"),
        ("bcastillo", "Beatriz Castillo", "backend", "occasional"),
        ("fdelgado", "Fernando Delgado", "devops", "occasional"),
        ("iherrera", "Isabel Herrera", "frontend", "occasional"),
        ("jortega", "Javier Ortega", "backend", "occasional"),
        ("kperez", "Karen Pérez", "data", "occasional"),
        ("lmunoz", "Luis Muñoz", "frontend", "occasional"),
        ("mruiz", "Miguel Ruiz", "backend", "occasional"),
        ("nvargas", "Natalia Vargas", "frontend", "occasional"),
        ("ocastro", "Óscar Castro", "devops", "occasional"),
        ("pgomez", "Patricia Gómez", "backend", "inactive"),
        ("qramos", "Quique Ramos", "frontend", "inactive"),
        ("rblanco", "Rosa Blanco", "data", "inactive"),
        ("smedina", "Sergio Medina", "backend", "inactive"),
        ("tgil", "Teresa Gil", "frontend", "inactive"),
        ("ualvarez", "Ulises Álvarez", "devops", "inactive"),
        ("vdominguez", "Verónica Domínguez", "backend", "neverused"),
        ("wmolina", "Wenceslao Molina", "frontend", "neverused"),
        ("xdiaz", "Xenia Díaz", "data", "neverused"),
        ("ysuarez", "Yolanda Suárez", "backend", "neverused"),
    ];

    private static readonly string[] Editors = ["vscode", "jetbrains", "neovim"];
    private static readonly string[] Languages = ["typescript", "csharp", "python", "java", "go", "rust", "javascript", "sql"];

    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var users = new List<User>();
        var usages = new List<DailyUsage>();
        var details = new List<DailyUsageDetail>();
        var aggregates = new Dictionary<DateOnly, (int active, int engaged, int suggestions, int acceptances)>();

        foreach (var (login, name, team, profile) in Devs)
        {
            users.Add(new User
            {
                UserLogin = login,
                DisplayName = name,
                Team = team,
                HasSeat = true,
                SeatAssignedDate = today.AddDays(-90),
                LastSyncedAt = DateTime.UtcNow,
            });

            for (int dayOffset = -27; dayOffset <= -2; dayOffset++)
            {
                var date = today.AddDays(dayOffset);
                var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

                var (isActive, isEngaged, sugBase, acceptRate, chatProb, agentProb) = profile switch
                {
                    "power" => (
                        !isWeekend && Rng.NextDouble() < 0.92,
                        !isWeekend && Rng.NextDouble() < 0.85,
                        Rng.Next(80, 200),
                        0.55 + Rng.NextDouble() * 0.25,
                        0.7,
                        0.4
                    ),
                    "occasional" => (
                        !isWeekend && Rng.NextDouble() < 0.45,
                        !isWeekend && Rng.NextDouble() < 0.30,
                        Rng.Next(20, 80),
                        0.30 + Rng.NextDouble() * 0.25,
                        0.3,
                        0.1
                    ),
                    "inactive" => (
                        !isWeekend && Rng.NextDouble() < 0.10,
                        !isWeekend && Rng.NextDouble() < 0.05,
                        Rng.Next(5, 25),
                        0.15 + Rng.NextDouble() * 0.20,
                        0.1,
                        0.0
                    ),
                    _ => (false, false, 0, 0.0, 0.0, 0.0),
                };

                if (!isActive && profile != "neverused")
                {
                    usages.Add(new DailyUsage
                    {
                        UserLogin = login,
                        Date = date,
                        IsActive = false,
                        IsEngaged = false,
                    });
                    continue;
                }

                if (profile == "neverused") continue;

                var suggestions = sugBase;
                var acceptances = (int)(suggestions * acceptRate);
                var linesSuggested = suggestions * Rng.Next(2, 5);
                var linesAccepted = (int)(linesSuggested * acceptRate * 0.9);
                var chatEngaged = Rng.NextDouble() < chatProb;
                var agentEngaged = Rng.NextDouble() < agentProb;
                var cliEngaged = Rng.NextDouble() < 0.05;

                var editor = Editors[Rng.Next(profile == "power" ? 2 : Editors.Length)];
                var lang = Languages[Rng.Next(profile == "power" ? 4 : Languages.Length)];

                usages.Add(new DailyUsage
                {
                    UserLogin = login,
                    Date = date,
                    IsActive = true,
                    IsEngaged = isEngaged,
                    CompletionsSuggestions = suggestions,
                    CompletionsAcceptances = acceptances,
                    CompletionsLinesSuggested = linesSuggested,
                    CompletionsLinesAccepted = linesAccepted,
                    ChatEngaged = chatEngaged,
                    AgentEngaged = agentEngaged,
                    CliEngaged = cliEngaged,
                    PrimaryEditor = editor,
                    PrimaryLanguage = lang,
                });

                // Add 1-3 language details per active day
                var numLangs = Rng.Next(1, 4);
                var usedLangs = Languages.OrderBy(_ => Rng.Next()).Take(numLangs).ToList();
                foreach (var l in usedLangs)
                {
                    var langSug = suggestions / numLangs + Rng.Next(-5, 5);
                    var langAcc = (int)(langSug * acceptRate);
                    details.Add(new DailyUsageDetail
                    {
                        UserLogin = login,
                        Date = date,
                        EditorName = editor,
                        LanguageName = l,
                        Suggestions = Math.Max(0, langSug),
                        Acceptances = Math.Max(0, langAcc),
                        LinesSuggested = Math.Max(0, langSug * Rng.Next(2, 5)),
                        LinesAccepted = Math.Max(0, langAcc * Rng.Next(2, 4)),
                    });
                }

                // Aggregate
                if (!aggregates.ContainsKey(date))
                    aggregates[date] = (0, 0, 0, 0);
                var agg = aggregates[date];
                aggregates[date] = (
                    agg.active + 1,
                    agg.engaged + (isEngaged ? 1 : 0),
                    agg.suggestions + suggestions,
                    agg.acceptances + acceptances
                );
            }
        }

        db.Users.AddRange(users);
        db.DailyUsages.AddRange(usages);
        db.DailyUsageDetails.AddRange(details);

        foreach (var (date, (active, engaged, sug, acc)) in aggregates)
        {
            db.DailyAggregates.Add(new DailyAggregate
            {
                Date = date,
                TotalActiveUsers = active,
                TotalEngagedUsers = engaged,
                TotalSuggestions = sug,
                TotalAcceptances = acc,
                AcceptanceRate = sug > 0 ? (decimal)acc / sug : 0,
            });
        }

        db.SaveChanges();
    }
}
