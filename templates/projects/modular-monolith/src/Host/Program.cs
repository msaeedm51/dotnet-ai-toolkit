using Inventory.Contracts;
using Inventory.Module;
using Orders.Contracts;
using Orders.Module;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

// Only the Host (composition root) knows about both modules' concrete implementations --
// see skills/architecture/modular-monolith.md.
builder.Services.AddSingleton<IOrdersModule, OrdersModule>();
builder.Services.AddSingleton<IInventoryModule, InventoryModule>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health/live");
app.MapOrdersEndpoints();
app.MapInventoryEndpoints();

app.Run();

public partial class Program { }
