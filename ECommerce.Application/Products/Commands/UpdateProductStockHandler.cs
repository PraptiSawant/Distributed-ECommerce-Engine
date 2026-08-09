using ECommerce.Application.Data;
using MediatR;

namespace ECommerce.Application.Products.Commands;

public class UpdateProductStockHandler : IRequestHandler<UpdateProductStockCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public UpdateProductStockHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the tracking instance from Neon Cloud
        var product = await _context.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);

        if (product == null) return false;

        try
        {
            // 2. Invoke our explicit Domain Business Rule (Triggers exceptions if stock drops below 0)
            product.UpdateStock(request.Quantity);

            // 3. Save the modified transaction back to PostgreSQL
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (InvalidOperationException)
        {
            // Fails gracefully if the domain validation rule is violated
            return false;
        }
    }
}