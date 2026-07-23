using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.Sales.Commands.CreateSale;

public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, Result<SaleDto>>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICurrentAccountRepository _currentAccountRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreateSaleCommandHandler(
        ISaleRepository saleRepository,
        ICurrentAccountRepository currentAccountRepository,
        IProductRepository productRepository,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _currentAccountRepository = currentAccountRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<SaleDto>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        // 1. CurrentAccount kontrolü
        var currentAccount = await _currentAccountRepository.GetByIdAsync(request.CurrentAccountId, cancellationToken);
        if (currentAccount is null)
        {
            return Result<SaleDto>.Fail($"CurrentAccount (Id={request.CurrentAccountId}) bulunamadı veya silinmiş.", 404);
        }

        // 2. Her ürünü validate et ve satır hesaplarını yap
        var lineItems = new List<SaleLineItem>();
        decimal totalAmount = 0m;
        decimal vatAmount = 0m;

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is null)
            {
                return Result<SaleDto>.Fail($"Ürün (Id={item.ProductId}) bulunamadı veya silinmiş.", 404);
            }

            if (product.StockQuantity < item.Quantity)
            {
                return Result<SaleDto>.Fail(
                    $"Ürün '{product.Name}' için yetersiz stok. Mevcut: {product.StockQuantity}, İstenen: {item.Quantity}.", 422);
            }

            var unitPrice = item.UnitPrice ?? product.SalePrice;
            var discountRate = item.Discount / 100m;
            var baseAmount = unitPrice * item.Quantity;
            var discountedAmount = baseAmount * (1m - discountRate);
            var lineVat = discountedAmount * (product.VatRate / 100m);

            totalAmount += discountedAmount;
            vatAmount += lineVat;

            lineItems.Add(new SaleLineItem(product, item.Quantity, unitPrice, item.Discount));
        }

        var grandTotal = totalAmount + vatAmount;

        // 3. Sale entity hazırla
        var sale = new Sale
        {
            CurrentAccountId = request.CurrentAccountId,
            SaleDate = DateTime.UtcNow,
            TotalAmount = Math.Round(totalAmount, 2),
            VatAmount = Math.Round(vatAmount, 2),
            GrandTotal = Math.Round(grandTotal, 2),
            Description = request.Description,
            CreatedDate = DateTime.UtcNow
        };

        // 4. Transaction içinde kaydet
        try
        {
            var created = await _saleRepository.CreateSaleAsync(sale, lineItems, currentAccount, cancellationToken);
            var dto = _mapper.Map<SaleDto>(created);
            return Result<SaleDto>.Created(dto, "Satış başarıyla oluşturuldu.");
        }
        catch (ConcurrencyConflictException)
        {
            return Result<SaleDto>.Fail("Eş zamanlı güncelleme tespit edildi. Lütfen tekrar deneyin.", 409);
        }
        catch (InvalidOperationException ex)
        {
            return Result<SaleDto>.Fail(ex.Message, 422);
        }
    }
}
