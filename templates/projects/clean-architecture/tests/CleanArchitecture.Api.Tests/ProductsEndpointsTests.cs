using System.Net;
using System.Net.Http.Json;
using CleanArchitecture.Api.Products;
using CleanArchitecture.Application.Products;
using Xunit;

namespace CleanArchitecture.Api.Tests;

public class ProductsEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Post_ValidRequest_Returns201()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/products", new CreateProductRequest("Widget", 9.99m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_NegativePrice_Returns400()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/products", new CreateProductRequest("Widget", -1m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_AfterCreate_IncludesTheNewProduct()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/products", new CreateProductRequest("Gadget", 19.99m));

        var response = await client.GetAsync("/products");
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(products!, p => p.Name == "Gadget");
    }
}
