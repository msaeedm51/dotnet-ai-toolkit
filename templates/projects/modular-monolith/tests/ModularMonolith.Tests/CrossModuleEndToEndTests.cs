using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ModularMonolith.Tests;

/// <summary>
/// Boots the real Host and proves the two modules interact correctly end to end through
/// the Host's composition root -- Orders creates an order, Inventory (via IOrdersModule)
/// validates it exists before reserving stock.
/// </summary>
public class CrossModuleEndToEndTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task CreateOrder_ThenReserveStock_Succeeds()
    {
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/orders", new { customerId = Guid.NewGuid() });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CreatedOrder>();

        var reserveResponse = await client.PostAsync($"/inventory/reserve/{created!.Id}", null);

        Assert.Equal(HttpStatusCode.OK, reserveResponse.StatusCode);
    }

    [Fact]
    public async Task ReserveStock_ForUnknownOrder_Returns404()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync($"/inventory/reserve/{Guid.NewGuid()}", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record CreatedOrder(Guid Id);
}
