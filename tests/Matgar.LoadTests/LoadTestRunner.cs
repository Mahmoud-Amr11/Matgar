using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace Matgar.LoadTests;

public static class LoadTestRunner
{
    private static readonly (string Step, double MaxFailPercent)[] FailureBudgets =
    [
        ("products_list", 1),
        ("categories", 1),
        ("product_details", 2),
        ("products_search", 1),
        ("authenticated_products", 1),
        ("auth_login", 1),
        ("health", 1)
    ];

    public static async Task<int> RunAsync(string[] args)
    {
        var options = LoadTestOptions.FromArgs(args);

        var api = new ApiClient(options.BaseUrl);
        var sample = await api.SampleCatalogAsync();
        var login = await api.LoginAsync(options.AdminEmail, options.AdminPassword);

        Console.WriteLine($"Load profile: {options.Profile}");
        Console.WriteLine($"Base URL:     {options.BaseUrl}");
        Console.WriteLine($"Sample data:  {sample.ProductIds.Length} products, {sample.CategoryIds.Length} categories");
        Console.WriteLine();

        using var httpClient = Http.CreateDefaultClient();

        var scenarios = Scenarios.Build(options, sample, login, httpClient);
        var runResult = NBomberRunner
            .RegisterScenarios(scenarios)
            .WithReportFolder(options.ReportFolder)
            .WithReportFileName(options.ReportName)
            .WithReportFormats(ReportFormat.Html, ReportFormat.Md, ReportFormat.Csv)
            .Run();

        var violations = CollectViolations(runResult);

        PrintMetricsSummary(runResult);

        Console.WriteLine();
        Console.WriteLine(violations.Count == 0
            ? "LOAD TEST PASSED: all thresholds satisfied."
            : $"LOAD TEST FAILED: {violations.Count} violation(s) detected.");
        foreach (var violation in violations)
            Console.WriteLine($"  - {violation}");

        Console.WriteLine($"Report: {Path.Combine(options.ReportFolder, options.ReportName)}");

        return violations.Count == 0 ? 0 : 1;
    }

    private static void PrintMetricsSummary(NodeStats stats)
    {
        foreach (var scenarioStats in stats.ScenarioStats)
        {
            foreach (var stepStats in scenarioStats.StepStats)
            {
                var ok = stepStats.Ok.Request.Count;
                var fail = stepStats.Fail.Request.Count;
                var total = ok + fail;
                var failPercent = total == 0 ? 0 : 100.0 * fail / total;

                var timeouts =
                    stepStats.Ok.StatusCodes.Where(s => s.StatusCode == "-100").Sum(s => s.Count) +
                    stepStats.Fail.StatusCodes.Where(s => s.StatusCode == "-100").Sum(s => s.Count);

                var codes = string.Join(",", stepStats.Ok.StatusCodes
                    .Concat(stepStats.Fail.StatusCodes)
                    .GroupBy(s => s.StatusCode)
                    .Select(g => $"{g.Key}:{g.Sum(s => s.Count)}"));

                Console.WriteLine(
                    $"[METRICS] scenario={scenarioStats.ScenarioName} step={stepStats.StepName} " +
                    $"requests={total} ok={ok} fail={fail} errorPct={failPercent:0.##}% " +
                    $"avg={stepStats.Ok.Latency.MeanMs:0.0}ms p50={stepStats.Ok.Latency.Percent50:0.0}ms " +
                    $"p95={stepStats.Ok.Latency.Percent95:0.0}ms p99={stepStats.Ok.Latency.Percent99:0.0}ms " +
                    $"max={stepStats.Ok.Latency.MaxMs:0.0}ms timeouts={timeouts} http=[{codes}]");
            }
        }
    }

    private static List<string> CollectViolations(NodeStats stats)
    {
        var violations = new List<string>();

        foreach (var threshold in stats.Thresholds.Where(t => t.IsFailed))
            violations.Add($"threshold failed: {threshold.CheckExpression}");

        foreach (var scenarioStats in stats.ScenarioStats)
        {
            foreach (var budget in FailureBudgets)
            {
                if (!scenarioStats.StepStats.Exists(budget.Step))
                    continue;

                var stepStats = scenarioStats.StepStats.Get(budget.Step);
                if (stepStats.Fail.Request.Percent > budget.MaxFailPercent)
                {
                    violations.Add(
                        $"{scenarioStats.ScenarioName}/{budget.Step}: fail {stepStats.Fail.Request.Percent:0.##}% " +
                        $"exceeds budget {budget.MaxFailPercent}%");
                }
            }
        }

        return violations;
    }
}