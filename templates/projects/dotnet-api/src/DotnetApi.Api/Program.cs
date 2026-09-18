using DotnetApi.Api.Items;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IItemStore, InMemoryItemStore>();
builder.Services.AddHealthChecks();

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
app.MapItemsEndpoints();

app.Run();

// Exposes the generated Program class to the test project's WebApplicationFactory<Program>.
public partial class Program { }
