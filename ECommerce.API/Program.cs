using ECommerce.Application.Data;
using ECommerce.Application.Products.Commands;
using ECommerce.Application.Products.Queries;
using ECommerce.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// 1. Add PostgreSQL Connection Context using modern C# connection rules
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductsQuery).Assembly));

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// 2. Automate database creation and seeding loop on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbSeeder.SeedDataAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 5. Create the live product collection endpoint route
app.MapGet("/api/products", async (ISender mediator, CancellationToken cancellationToken) =>
{
    var query = new GetProductsQuery();
    var result = await mediator.Send(query, cancellationToken);

    return result is not null ? Results.Ok(result) : Results.NotFound();
})
.WithName("GetProducts")
.WithOpenApi();

app.MapPost("/api/products/update-stock", async (UpdateProductStockCommand command, ISender mediator) =>
{
    var success = await mediator.Send(command);
    return success ? Results.Ok(new { Message = "Stock updated successfully." }) : Results.BadRequest("Invalid request or insufficient stock.");
})
.WithName("UpdateStock")
.WithOpenApi();

// Basic placeholder health route
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Engine = ".NET 9/10 Core" }));

app.Run();
