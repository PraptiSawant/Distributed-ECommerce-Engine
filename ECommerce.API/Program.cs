using ECommerce.Application.Common.Events;
using ECommerce.Application.Data;
using ECommerce.Application.Products.Commands;
using ECommerce.Application.Products.Queries;
using ECommerce.Infrastructure.Data;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// 1. Add PostgreSQL Connection Context using modern C# connection rules
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductsQuery).Assembly));

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        // Aspire automatically injects the RabbitMQ host address via connection string mapping
        var rabbitUri = builder.Configuration.GetConnectionString("messaging");
        cfg.Host(rabbitUri);
    });
});

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

app.MapPost("/api/products/checkout", async (Guid productId, int quantity, IPublishEndpoint publishEndpoint) =>
{
    // Validation check: ensure they aren't purchasing a negative or zero quantity
    if (quantity <= 0) return Results.BadRequest("Quantity purchased must be greater than zero.");

    // 1. Instantly create our integration event data envelope
    var integrationEvent = new OrderSubmittedEvent(productId, quantity);

    // 2. Drop the envelope into the RabbitMQ message queue line (Takes less than 2 milliseconds!)
    await publishEndpoint.Publish(integrationEvent);

    // 3. Return a clean 202 Accepted status code to set enterprise expectations
    return Results.Accepted(value: new
    {
        Message = "Order received and is now processing in the checkout queue pipeline.",
        TargetProductId = productId
    });
})
.WithName("CheckoutProduct")
.WithOpenApi();

// Basic placeholder health route
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Engine = ".NET 9/10 Core" }));

app.Run();
