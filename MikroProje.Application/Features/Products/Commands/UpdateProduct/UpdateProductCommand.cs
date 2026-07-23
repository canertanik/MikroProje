using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;

namespace MikroProje.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest<Result<ProductDto>>
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SalePrice { get; set; }

    public decimal VatRate { get; set; }

    public int CriticalStockQuantity { get; set; }
}
