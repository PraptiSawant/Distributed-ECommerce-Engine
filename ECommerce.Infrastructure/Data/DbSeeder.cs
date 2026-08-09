using ECommerce.Application.Data;
using ECommerce.Domain.Entities;
using System.Net.Http.Json;

namespace ECommerce.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedDataAsync(ApplicationDbContext context)
    {
        // 1. Ensure database exists and schema is up to date
        await context.Database.EnsureCreatedAsync();

        // 2. Short-circuit if database is already populated
        if (context.Products.Any()) return;

        using var httpClient = new HttpClient();

        try
        {
            // 3. Fetch realistic mock store datasets
            var response = await httpClient.GetFromJsonAsync<DummyJsonResponse>("https://dummyjson.com/products");
            Console.WriteLine($"[DIAGNOSTIC] DummyJSON Raw Payload: {response}");

            if (response?.Products != null)
            {
                var productsToInsert = response.Products.Select(p => new Product(
                    Guid.NewGuid(),
                    p.Title,
                    p.Description,
                    p.Price,
                    p.Stock > 0 ? p.Stock : 50, // Fallback safety stock
                    p.Thumbnail,
                    p.Category
                )).ToList();

                // 4. Batch insert into PostgreSQL
                await context.Products.AddRangeAsync(productsToInsert);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception)
        {
            // Fail-safe default seeding if external network is down
            var fallbackProduct = new Product(
                Guid.NewGuid(),
                "Fallback Modern Mouse",
                "Backup development seeding data listing.",
                49.99m,
                100,
                "https://placeholder.com",
                "electronics"
            );
            await context.Products.AddAsync(fallbackProduct);
            await context.SaveChangesAsync();
        }
    }
}

// Internal Data Transfer Objects matching DummyJSON format structure
internal record DummyJsonResponse(List<DummyJsonProductDto> Products);
internal record DummyJsonProductDto(string Title, string Description, decimal Price, int Stock, string Thumbnail, string Category);

