namespace DotnetApi.Api.Items;

public static class ItemsEndpoints
{
    public static void MapItemsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/items").WithTags("Items");

        group.MapGet("/", (IItemStore store) => Results.Ok(store.GetAll()))
            .WithName("ListItems");

        group.MapGet("/{id:guid}", (Guid id, IItemStore store) =>
        {
            var item = store.GetById(id);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithName("GetItem");

        group.MapPost("/", (CreateItemRequest request, IItemStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["name"] = ["Name is required."],
                });
            }

            var item = store.Add(request.Name);
            return Results.Created($"/items/{item.Id}", item);
        })
        .WithName("CreateItem");
    }
}

public sealed record CreateItemRequest(string Name);

public sealed record ItemDto(Guid Id, string Name, DateTimeOffset CreatedAtUtc);

public interface IItemStore
{
    IReadOnlyList<ItemDto> GetAll();
    ItemDto? GetById(Guid id);
    ItemDto Add(string name);
}

public sealed class InMemoryItemStore : IItemStore
{
    private readonly List<ItemDto> _items = [];
    private readonly object _lock = new();

    public IReadOnlyList<ItemDto> GetAll()
    {
        lock (_lock) return _items.ToList();
    }

    public ItemDto? GetById(Guid id)
    {
        lock (_lock) return _items.FirstOrDefault(i => i.Id == id);
    }

    public ItemDto Add(string name)
    {
        var item = new ItemDto(Guid.NewGuid(), name, DateTimeOffset.UtcNow);
        lock (_lock) _items.Add(item);
        return item;
    }
}
