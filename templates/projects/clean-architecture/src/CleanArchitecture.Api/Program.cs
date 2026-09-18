using CleanArchitecture.Api.Products;
using CleanArchitecture.Application.Products;
using CleanArchitecture.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=app.db";
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<GetProductsHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Dev convenience only -- a real project would use migrations (see
    // skills/data/efcore-migrations.md) instead of EnsureCreated for anything beyond
    // a template/demo.
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health/live");
app.MapProductsEndpoints();

app.Run();

public partial class Program { }
