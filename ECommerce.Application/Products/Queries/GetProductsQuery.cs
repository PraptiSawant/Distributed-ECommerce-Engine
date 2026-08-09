using MediatR;

namespace ECommerce.Application.Products.Queries;

public record GetProductsQuery : IRequest<List<ProductDto>>;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string ImageUrl,
    string Category
);
