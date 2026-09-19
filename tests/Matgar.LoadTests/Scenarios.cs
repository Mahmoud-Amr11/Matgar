using NBomber.Contracts;
using NBomber.CSharp;
using NBomber.Http;
using NBomber.Http.CSharp;

namespace Matgar.LoadTests;

public static class Scenarios
{
    public static ScenarioProps[] Build(
        LoadTestOptions options,
        CatalogSample sample,
        LoginToken login,
        HttpClient httpClient)
    {
        if (options.SelectedScenario is { } scenarioName)
            return BuildSelected(options, scenarioName, sample, login, httpClient);

        var catalog = BuildCatalogScenarios(options, sample, httpClient);
        var authenticated = BuildAuthenticatedScenario(options, login, httpClient);
        var auth = BuildAuthScenario(options, httpClient);
        var health = BuildHealthScenario(options, httpClient);

        return [catalog.Browsing, catalog.Details, catalog.Search, authenticated, auth, health];
    }

    private static ScenarioProps[] BuildSelected(
        LoadTestOptions options,
        string scenarioName,
        CatalogSample sample,
        LoginToken login,
        HttpClient httpClient)
    {
        var catalog = BuildCatalogScenarios(options, sample, httpClient);
        var authenticated = BuildAuthenticatedScenario(options, login, httpClient);
        var auth = BuildAuthScenario(options, httpClient);
        var health = BuildHealthScenario(options, httpClient);

        return scenarioName switch
        {
            "catalog_browsing" => [catalog.Browsing],
            "catalog_details" => [catalog.Details],
            "catalog_search" => [catalog.Search],
            "authenticated_catalog" => [authenticated],
            "auth_login" => [auth],
            "health" => [health],
            _ => throw new ArgumentException(
                $"Unknown scenario '{scenarioName}'. Expected one of: " +
                "catalog_browsing, catalog_details, catalog_search, authenticated_catalog, auth_login, health")
        };
    }

    private static LoadSimulation[] ClosedModel(LoadTestOptions options) =>
    [
        Simulation.RampingConstant(copies: options.BrowsingCopies, during: options.RampDuration),
        Simulation.KeepConstant(copies: options.BrowsingCopies, during: options.SteadyDuration)
    ];

    private static bool IsSingleSelection(LoadTestOptions options) =>
        options.SelectedScenario is not null;

    private static CatalogScenarios BuildCatalogScenarios(
        LoadTestOptions options,
        CatalogSample sample,
        HttpClient httpClient)
    {
        var browsing = Scenario.Create("catalog_browsing", async context =>
        {
            var page = Random.Shared.Next(1, 6);
            var listResponse = await Step.Run("products_list", context, async () =>
            {
                var request = Http.CreateRequest("GET", $"{options.BaseUrl}/api/v1/products?page={page}&pageSize=20");
                return await Http.Send(httpClient, CreateClientArgs(), request);
            });

            if (sample.CategoryIds.Length > 0)
            {
                var categoryId = sample.CategoryIds[Random.Shared.Next(0, sample.CategoryIds.Length)];
                await Step.Run("category_products", context, async () =>
                {
                    var request = Http.CreateRequest("GET", $"{options.BaseUrl}/api/v1/products?categoryId={categoryId}&pageSize=20");
                    return await Http.Send(httpClient, CreateClientArgs(), request);
                });
            }

            return await Step.Run("categories", context, async () =>
            {
                var request = Http.CreateRequest("GET", $"{options.BaseUrl}/api/v1/categories?page=1&pageSize=100");
                return await Http.Send(httpClient, CreateClientArgs(), request);
            });
        })
        .WithWarmUpDuration(options.WarmUpDuration)
        .WithLoadSimulations(
            Simulation.RampingConstant(copies: options.BrowsingCopies, during: options.RampDuration),
            Simulation.KeepConstant(copies: options.BrowsingCopies, during: options.SteadyDuration))
        .WithMaxFailCount(int.MaxValue)
        .WithThresholds(
            Threshold.Create("products_list", stats => stats.Fail.Request.Percent < 1),
            Threshold.Create("products_list", stats => stats.Ok.Latency.Percent95 < 2_000),
            Threshold.Create("categories", stats => stats.Fail.Request.Percent < 1));

        var details = Scenario.Create("catalog_details", async context =>
        {
            return await Step.Run("product_details", context, async () =>
            {
                if (sample.ProductIds.Length == 0)
                {
                    var request = Http.CreateRequest("GET", $"{options.BaseUrl}/api/v1/products?page=1&pageSize=20");
                    return await Http.Send(httpClient, CreateClientArgs(), request);
                }

                var productId = sample.ProductIds[Random.Shared.Next(0, sample.ProductIds.Length)];
                var productRequest = Http.CreateRequest("GET", $"{options.BaseUrl}/api/v1/products/{productId}");
                return await Http.Send(httpClient, CreateClientArgs(), productRequest);
            });
        })
        .WithWarmUpDuration(options.WarmUpDuration)
        .WithLoadSimulations(IsSingleSelection(options)
            ? ClosedModel(options)
            : [Simulation.Inject(
                rate: options.InjectedRate,
                interval: TimeSpan.FromSeconds(1),
                during: options.SteadyDuration)])
        .WithMaxFailCount(int.MaxValue)
        .WithThresholds(
            Threshold.Create("product_details", stats => stats.Fail.Request.Percent < 2));

        var search = Scenario.Create("catalog_search", async context =>
        {
            return await Step.Run("products_search", context, async () =>
            {
                var term = new[] { "phone", "laptop", "shirt", "shoe" }[Random.Shared.Next(0, 4)];
                var request = Http.CreateRequest("GET", $"{options.BaseUrl}/api/v1/products?search={term}&page=1&pageSize=12");
                return await Http.Send(httpClient, CreateClientArgs(), request);
            });
        })
        .WithWarmUpDuration(options.WarmUpDuration)
        .WithLoadSimulations(IsSingleSelection(options)
            ? ClosedModel(options)
            : [Simulation.Inject(
                rate: options.InjectedRate,
                interval: TimeSpan.FromSeconds(1),
                during: options.SteadyDuration)])
        .WithMaxFailCount(int.MaxValue)
        .WithThresholds(
            Threshold.Create("products_search", stats => stats.Fail.Request.Percent < 1));

        return new CatalogScenarios(browsing, details, search);
    }

    private sealed record CatalogScenarios(
        ScenarioProps Browsing,
        ScenarioProps Details,
        ScenarioProps Search);

    private static ScenarioProps BuildAuthenticatedScenario(
        LoadTestOptions options,
        LoginToken login,
        HttpClient httpClient)
    {
        return Scenario.Create("authenticated_catalog", async context =>
        {
            return await Step.Run("authenticated_products", context, async () =>
            {
                var request = Http.CreateRequest("GET", $"{options.BaseUrl}/api/v1/products?page=1&pageSize=20");
                if (!string.IsNullOrEmpty(login.AccessToken))
                    request.WithHeader("Authorization", $"Bearer {login.AccessToken}");

                return await Http.Send(httpClient, CreateClientArgs(), request);
            });
        })
        .WithWarmUpDuration(options.WarmUpDuration)
        .WithLoadSimulations(IsSingleSelection(options)
            ? ClosedModel(options)
            : [Simulation.KeepConstant(
                copies: Math.Max(1, options.BrowsingCopies / 5),
                during: options.SteadyDuration)])
        .WithMaxFailCount(int.MaxValue)
        .WithThresholds(
            Threshold.Create("authenticated_products", stats => stats.Fail.Request.Percent < 1));
    }

    private static ScenarioProps BuildAuthScenario(LoadTestOptions options, HttpClient httpClient)
    {
        return Scenario.Create("auth", async context =>
        {
            return await Step.Run("auth_login", context, async () =>
            {
                var request = Http.CreateRequest("POST", $"{options.BaseUrl}/api/v1/auth/login")
                    .WithJsonBody(new { email = options.AdminEmail, password = options.AdminPassword });

                return await Http.Send(httpClient, CreateClientArgs(), request);
            });
        })
        .WithWarmUpDuration(options.WarmUpDuration)
        .WithLoadSimulations(IsSingleSelection(options)
            ? ClosedModel(options)
            : [Simulation.Inject(
                rate: 1,
                interval: TimeSpan.FromSeconds(1),
                during: TimeSpan.FromSeconds(20))])
        .WithMaxFailCount(int.MaxValue)
        .WithThresholds(
            Threshold.Create("auth_login", stats => stats.Fail.Request.Percent < 1));
    }

    private static ScenarioProps BuildHealthScenario(LoadTestOptions options, HttpClient httpClient)
    {
        return Scenario.Create("health", async context =>
        {
            return await Step.Run("health", context, async () =>
            {
                var request = Http.CreateRequest("GET", $"{options.BaseUrl}/health");
                return await Http.Send(httpClient, CreateClientArgs(), request);
            });
        })
        .WithWarmUpDuration(options.WarmUpDuration)
        .WithLoadSimulations(IsSingleSelection(options)
            ? ClosedModel(options)
            : [Simulation.KeepConstant(
                copies: Math.Min(10, options.BrowsingCopies),
                during: options.SteadyDuration)])
        .WithMaxFailCount(int.MaxValue)
        .WithThresholds(
            Threshold.Create("health", stats => stats.Fail.Request.Percent < 1));
    }

    private static HttpClientArgs CreateClientArgs()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        return HttpClientArgs.Create(timeout.Token);
    }
}