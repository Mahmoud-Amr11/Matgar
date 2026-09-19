using System.Net.Http.Json;

namespace Matgar.LoadTests;

public sealed record CatalogSample(string[] ProductIds, string[] CategoryIds);

public sealed record LoginToken(string AccessToken, string UserId);

public sealed class ApiClient
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public ApiClient(string baseUrl)
    {
        BaseUrl = baseUrl;
    }

    public string BaseUrl { get; }

    public async Task<CatalogSample> SampleCatalogAsync()
    {
        var products = await _http.GetFromJsonAsync<Paged<ProductListItem>>(
            $"{BaseUrl}/api/v1/products?page=1&pageSize=20");

        var categories = await _http.GetFromJsonAsync<Paged<CategoryItem>>(
            $"{BaseUrl}/api/v1/categories?page=1&pageSize=100");

        return new CatalogSample(
            ProductIds: products?.Items?.Select(p => p.ProductId.ToString()).ToArray() ?? Array.Empty<string>(),
            CategoryIds: categories?.Items?.Select(c => c.CategoryId.ToString()).ToArray() ?? Array.Empty<string>());
    }

    public async Task<LoginToken> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync(
            $"{BaseUrl}/api/v1/auth/login",
            new { email, password });

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[warn] login failed with {response.StatusCode}; authenticated scenarios will run with an empty token.");
            return new LoginToken(string.Empty, string.Empty);
        }

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return new LoginToken(body?.AccessToken ?? string.Empty, body?.UserId ?? string.Empty);
    }

    private sealed record AuthResponse(string UserId, string Email, string AccessToken, DateTime AccessTokenExpiresAt);

    private sealed record Paged<T>(IReadOnlyList<T> Items);

    private sealed record ProductListItem(Guid ProductId, string ProductName, string CategoryName, Guid CategoryId, int Status, decimal MinPrice, decimal MaxPrice, string? ThumbnailUrl);

    private sealed record CategoryItem(Guid CategoryId, string CategorySlug, string CategoryName);
}