using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;

namespace RestaurantOrders.Integration.Tests;

public sealed class ApiSmokeTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"restaurant-tests-{Guid.NewGuid():N}.db");
    private WebApplicationFactory<Program> _factory = null!;

    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}"
                })));
        return Task.CompletedTask;
    }

    [Fact]
    public async Task HealthAndRestaurantSearch_AreAvailableOnFreshDatabase()
    {
        using var client = _factory.CreateClient();

        var health = await client.GetAsync("/health");
        var restaurants = await client.GetFromJsonAsync<SearchResponse>("/api/v1/restaurants");

        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.NotNull(restaurants);
        Assert.Equal(6, restaurants.TotalCount);
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        if (File.Exists(_databasePath))
            File.Delete(_databasePath);
    }

    private sealed record SearchResponse(int TotalCount);
}