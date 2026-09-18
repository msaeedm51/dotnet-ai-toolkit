using System.Net;
using System.Net.Http.Json;
using DotnetApi.Api.Items;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DotnetApi.Api.Tests;

public class ItemsEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Post_ValidRequest_Returns201WithLocation()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/items", new CreateItemRequest("First item"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Post_MissingName_Returns400ValidationProblem()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/items", new CreateItemRequest(""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_UnknownId_Returns404()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/items/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAndPost_RoundTrip_ItemIsRetrievable()
    {
        var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/items", new CreateItemRequest("Round trip item"));
        var item = await created.Content.ReadFromJsonAsync<ItemDto>();

        var fetched = await client.GetAsync($"/items/{item!.Id}");

        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
    }

    [Fact]
    public async Task HealthLive_ReturnsHealthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
