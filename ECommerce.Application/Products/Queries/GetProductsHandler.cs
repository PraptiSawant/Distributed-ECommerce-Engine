using ECommerce.Application.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Products.Queries;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly ApplicationDbContext _context;

    // Inject our PostgreSQL DbContext directly into the handler
    public GetProductsHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // Fetch all products from Neon Cloud using high-performance, read-only tracking
        var products = await _context.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Map database entities directly into lightweight DTO records
        return products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.StockQuantity,
            p.ImageUrl,
            p.Category
        )).ToList();
    }
}
