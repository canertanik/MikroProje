using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.Purchases.Commands.CreatePurchase;

public class CreatePurchaseCommandHandler : IRequestHandler<CreatePurchaseCommand, Result<PurchaseDto>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICurrentAccountRepository _currentAccountRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreatePurchaseCommandHandler(
        IPurchaseRepository purchaseRepository,
        ICurrentAccountRepository currentAccountRepository,
        IProductRepository productRepository,
        IMapper mapper)
    {
        _purchaseRepository = purchaseRepository;
        _currentAccountRepository = currentAccountRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<PurchaseDto>> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
    {
        // 1. CurrentAccount doğrulama
        var currentAccount = await _currentAccountRepository.GetByIdAsync(request.CurrentAccountId, cancellationToken);
        if (currentAccount is null)
        {
            return Result<PurchaseDto>.Fail($"CurrentAccount (Id={request.CurrentAccountId}) bulunamadı veya silinmiş.", 404);
        }

        if (currentAccount.Type != CurrentAccountType.Supplier)
        {
            return Result<PurchaseDto>.Fail($"Seçilen cari hesap ({currentAccount.Name}) bir Tedarikçi değil.", 400);
        }

        // 2. Ürün ve satır hesaplamaları
        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
        var productDict = products.ToDictionary(p => p.Id);

        var lineItems = new List<PurchaseItem>();
        decimal subtotal = 0m;
        decimal vatAmount = 0m;

        foreach (var item in request.Items)
        {
            if (!productDict.TryGetValue(item.ProductId, out var product))
            {
                return Result<PurchaseDto>.Fail($"Ürün (Id={item.ProductId}) bulunamadı veya silinmiş.", 404);
            }

            var unitPrice = item.UnitPrice ?? product.PurchasePrice; // Satın almada PurchasePrice referans alınabilir
            var lineSubtotal = unitPrice * item.Quantity;
            var lineVat = lineSubtotal * (product.VatRate / 100m);
            var lineTotal = lineSubtotal + lineVat;

            subtotal += lineSubtotal;
            vatAmount += lineVat;

            lineItems.Add(new PurchaseItem
            {
                Product = product, // Geçici olarak tutuluyor, ID'yi aşağıda repository alacak
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                VatRate = product.VatRate,
                LineSubtotal = Math.Round(lineSubtotal, 2),
                VatAmount = Math.Round(lineVat, 2),
                LineTotal = Math.Round(lineTotal, 2)
            });
        }

        var grandTotal = subtotal + vatAmount;
        var purchaseDate = request.PurchaseDate ?? DateTime.UtcNow;
        if (purchaseDate.Kind == DateTimeKind.Local) purchaseDate = purchaseDate.ToUniversalTime();
        else if (purchaseDate.Kind == DateTimeKind.Unspecified) purchaseDate = DateTime.SpecifyKind(purchaseDate, DateTimeKind.Utc);

        // 3. Purchase Entity oluşturma
        var purchase = new Purchase
        {
            CurrentAccountId = request.CurrentAccountId,
            PurchaseDate = purchaseDate,
            Subtotal = Math.Round(subtotal, 2),
            VatAmount = Math.Round(vatAmount, 2),
            GrandTotal = Math.Round(grandTotal, 2),
            Description = request.Description,
            CreatedDate = DateTime.UtcNow
        };

        // 4. Transaction içinde kaydetme
        try
        {
            var created = await _purchaseRepository.CreatePurchaseAsync(purchase, lineItems, currentAccount, cancellationToken);
            var dto = _mapper.Map<PurchaseDto>(created);
            return Result<PurchaseDto>.Created(dto, "Satın alma işlemi başarıyla tamamlandı.");
        }
        catch (ConcurrencyConflictException)
        {
            return Result<PurchaseDto>.Fail("Eş zamanlı güncelleme tespit edildi. Lütfen tekrar deneyin.", 409);
        }
    }
}
