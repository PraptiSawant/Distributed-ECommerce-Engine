namespace ECommerce.Application.Products.Commands;

using MediatR;

// This record takes parameters and returns a boolean (True if successful)
public record UpdateProductStockCommand(Guid ProductId, int Quantity) : IRequest<bool>;

