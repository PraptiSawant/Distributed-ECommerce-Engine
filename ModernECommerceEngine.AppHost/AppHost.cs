using Microsoft.AspNetCore.Hosting.Server;

var builder = DistributedApplication.CreateBuilder(args);

// 1. Tell Aspire to register your existing Cloud Neon database pointer
var commerceDb = builder.AddConnectionString("PostgreSQL");

var rabbitMq = builder.AddConnectionString("messaging");

var apiServer = builder.AddProject<Projects.ECommerce_API>("ecommerce-api")
       .WithReference(commerceDb)
       .WithReference(rabbitMq);

builder.AddProject<Projects.ECommerce_InventoryWorker>("ecommerce-inventoryworker")
       .WithReference(commerceDb)
       .WithReference(rabbitMq);

builder.AddProject<Projects.ECommerce_UI>("ecommerce-ui")
       .WithReference(apiServer);

builder.Build().Run();
