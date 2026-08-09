var builder = DistributedApplication.CreateBuilder(args);

// 1. Tell Aspire to register your existing Cloud Neon database pointer
var commerceDb = builder.AddConnectionString("PostgreSQL");

// 2. Automatically bind and pass this connection downstream into your API project configuration
builder.AddProject<Projects.ECommerce_API>("ecommerce-api")
       .WithReference(commerceDb);

builder.Build().Run();
