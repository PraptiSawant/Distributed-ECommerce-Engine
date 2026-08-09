using ECommerce.Application.Data;
using ECommerce.InventoryWorker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Configure MassTransit over RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Auto-discover and register our OrderSubmittedConsumer class layout
    x.AddConsumer<OrderSubmittedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Aspire automatically tells us where RabbitMQ is running via connections configuration
        var hostUri = builder.Configuration.GetConnectionString("messaging");
        cfg.Host(hostUri);

        // Setup the queue configuration topology automatically
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
