using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IProductRepository productRepository, IMapper mapper, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _productRepository.CodeExistsAsync(request.Code, null, cancellationToken);
        if (codeExists)
        {
            return Result<ProductDto>.Fail("Product code already exists.", 409);
        }

        var barcodeExists = await _productRepository.BarcodeExistsAsync(request.Barcode, null, cancellationToken);
        if (barcodeExists)
        {
            return Result<ProductDto>.Fail("Product barcode already exists.", 409);
        }

        var product = new Product
        {
            Code = request.Code,
            Name = request.Name,
            Barcode = request.Barcode,
            PurchasePrice = request.PurchasePrice,
            SalePrice = request.SalePrice,
            VatRate = request.VatRate,
            CriticalStockQuantity = request.CriticalStockQuantity,
            StockQuantity = 0,
            CreatedDate = DateTime.UtcNow
        };

        try
        {
            var createdProduct = await _productRepository.CreateWithInitialStockAsync(product, request.InitialStockQuantity, cancellationToken);
            var dto = _mapper.Map<ProductDto>(createdProduct);
            return Result<ProductDto>.Created(dto, "Product created successfully.");
        }
        catch (ConcurrencyConflictException)
        {
            return Result<ProductDto>.Fail("Concurrent update detected.", 409);
        }
    }
}
