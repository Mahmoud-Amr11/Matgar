namespace Matgar.LoadTests;

public enum LoadProfile
{
    Smoke,
    Standard,
    Heavy
}

public sealed record LoadTestOptions(
    string BaseUrl,
    LoadProfile Profile,
    string AdminEmail,
    string AdminPassword,
    string ReportFolder,
    string ReportName,
    int? Copies,
    string? SelectedScenario)
{
    public static LoadTestOptions FromArgs(string[] args)
    {
        var baseUrl = Get(args, "--base-url") ?? "http://localhost:5203";

        var profile = args.Contains("--smoke") ? LoadProfile.Smoke
            : args.Contains("--heavy") ? LoadProfile.Heavy
            : LoadProfile.Standard;

        var adminEmail = Environment.GetEnvironmentVariable("MATGAR_ADMIN_EMAIL") ?? "Admin@Admin.com";
        var adminPassword = Environment.GetEnvironmentVariable("MATGAR_ADMIN_PASSWORD") ?? "Admin@12345";

        var copiesArg = Get(args, "--copies");
        int? copies = int.TryParse(copiesArg, out var resolvedCopies) && resolvedCopies > 0
            ? resolvedCopies
            : null;

        var scenarioName = Get(args, "--scenario");

        return new LoadTestOptions(
            BaseUrl: baseUrl,
            Profile: profile,
            AdminEmail: adminEmail,
            AdminPassword: adminPassword,
            ReportFolder: Path.Combine(Environment.CurrentDirectory, "reports"),
            ReportName: $"matgar-load-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
            Copies: copies,
            SelectedScenario: scenarioName);
    }

    // Browsing concurrency for the steady state of read scenarios.
    public int BrowsingCopies => Copies ?? Profile switch
    {
        LoadProfile.Smoke => 5,
        LoadProfile.Heavy => 200,
        _ => 50
    };

    // Requests per second injected into the search / details scenarios.
    public int InjectedRate => Profile switch
    {
        LoadProfile.Smoke => 10,
        LoadProfile.Heavy => 300,
        _ => 100
    };

    public TimeSpan RampDuration => Profile == LoadProfile.Smoke
        ? TimeSpan.FromSeconds(10)
        : TimeSpan.FromSeconds(15);

    public TimeSpan SteadyDuration => Profile == LoadProfile.Smoke
        ? TimeSpan.FromSeconds(30)
        : TimeSpan.FromSeconds(60);

    public TimeSpan WarmUpDuration => Profile == LoadProfile.Smoke
        ? TimeSpan.Zero
        : TimeSpan.FromSeconds(5);

    private static string? Get(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        if (index < 0 || index + 1 >= args.Length)
            return null;

        return args[index + 1];
    }
}