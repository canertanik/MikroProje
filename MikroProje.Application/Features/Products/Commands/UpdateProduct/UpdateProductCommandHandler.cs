using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IProductRepository productRepository, IMapper mapper, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result<ProductDto>.Fail("Product not found.", 404);
        }

        var codeExists = await _productRepository.CodeExistsAsync(request.Code, request.Id, cancellationToken);
        if (codeExists)
        {
            return Result<ProductDto>.Fail("Product code already exists.", 409);
        }

        var barcodeExists = await _productRepository.BarcodeExistsAsync(request.Barcode, request.Id, cancellationToken);
        if (barcodeExists)
        {
            return Result<ProductDto>.Fail("Product barcode already exists.", 409);
        }

        product.Code = request.Code;
        product.Name = request.Name;
        product.Barcode = request.Barcode;
        product.PurchasePrice = request.PurchasePrice;
        product.SalePrice = request.SalePrice;
        product.VatRate = request.VatRate;
        product.CriticalStockQuantity = request.CriticalStockQuantity;
        product.UpdatedDate = DateTime.UtcNow;

        try
        {
            await _productRepository.SaveChangesAsync(cancellationToken);
            var dto = _mapper.Map<ProductDto>(product);
            return Result<ProductDto>.Ok(dto, "Product updated successfully.");
        }
        catch (ConcurrencyConflictException)
        {
            return Result<ProductDto>.Fail("Concurrent update detected.", 409);
        }
    }
}
