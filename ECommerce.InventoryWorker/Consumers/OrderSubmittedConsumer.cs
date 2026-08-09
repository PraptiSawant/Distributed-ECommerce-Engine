using ECommerce.Application.Data; 
using ECommerce.Application.Common.Events;
using MassTransit;

namespace ECommerce.InventoryWorker.Consumers;

// Implementing IConsumer tells MassTransit to route matching queue events here
public class OrderSubmittedConsumer : IConsumer<OrderSubmittedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderSubmittedConsumer> _logger;

    public OrderSubmittedConsumer(ApplicationDbContext context, ILogger<OrderSubmittedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderSubmittedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("🔀 Processing queued background inventory logic for Product ID: {ProductId}", message.ProductId);

        // 1. Fetch tracking instance straight from Neon Cloud
        var product = await _context.Products.FindAsync(new object[] { message.ProductId }, context.CancellationToken);

        if (product == null)
        {
            _logger.LogError("❌ Processing aborted: Product ID {ProductId} not found in database.", message.ProductId);
            return;
        }

        try
        {
            // 2. Invoke our encapsulated Domain Rule (Passes negative value to deduct stock)
            product.UpdateStock(-message.QuantityPurchased);

            // 3. Persist the database record update down to PostgreSQL
            await _context.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("✅ Background stock allocation successful for {ProductName}. Remaining: {NewStock}",
                product.Name, product.StockQuantity);
        }
        catch (InvalidOperationException ex)
        {
            // Handles cases where stock drops below zero sequentially
            _logger.LogWarning("⚠️ Stock allocation rejected: {Message}", ex.Message);

            // NOTE: This is where we would trigger an alternative system event later 
            // to refund a customer or send an "Out of stock" live signal via SignalR!
        }
    }
}
