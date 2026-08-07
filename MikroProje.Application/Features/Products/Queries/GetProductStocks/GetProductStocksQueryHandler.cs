using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Products.Queries.GetProductStocks;

public class GetProductStocksQueryHandler : IRequestHandler<GetProductStocksQuery, Result<List<ProductStockDto>>>
{
    private readonly IProductRepository _productRepository;

    public GetProductStocksQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<ProductStockDto>>> Handle(GetProductStocksQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<List<ProductStockDto>>.Fail("Ürün bulunamadı.", 404);
        }

        var stocks = await _productRepository.GetWarehouseStocksAsync(request.ProductId, cancellationToken);

        var dtos = stocks.Select(s => new ProductStockDto
        {
            WarehouseId = s.WarehouseId,
            WarehouseCode = s.Warehouse.Code,
            WarehouseName = s.Warehouse.Name,
            Quantity = s.Quantity
        }).ToList();

        var warehouseStockSum = dtos.Sum(x => x.Quantity);
        if (product.StockQuantity > warehouseStockSum)
        {
            dtos.Add(new ProductStockDto
            {
                WarehouseId = 0,
                WarehouseCode = "SISTEM",
                WarehouseName = "Genel Stok (Depo Ataması Yok)",
                Quantity = product.StockQuantity - warehouseStockSum
            });
        }

        return Result<List<ProductStockDto>>.Ok(dtos);
    }
}
