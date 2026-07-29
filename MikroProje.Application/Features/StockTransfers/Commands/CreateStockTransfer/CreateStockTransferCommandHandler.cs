using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Application.Features.StockTransfers.DTOs;

namespace MikroProje.Application.Features.StockTransfers.Commands.CreateStockTransfer;

public class CreateStockTransferCommandHandler : IRequestHandler<CreateStockTransferCommand, Result<StockTransferDto>>
{
    private readonly ICacheService _cacheService;
    private readonly IStockTransferRepository _repository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreateStockTransferCommandHandler(IStockTransferRepository repository, IWarehouseRepository warehouseRepository, IProductRepository productRepository, IMapper mapper, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _repository = repository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<StockTransferDto>> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (dto.SourceWarehouseId == dto.DestinationWarehouseId)
        {
            return Result<StockTransferDto>.Fail("Kaynak ve hedef depo ayný olamaz.", 400);
        }

        var sourceWarehouse = await _warehouseRepository.GetByIdAsync(dto.SourceWarehouseId, cancellationToken);
        if (sourceWarehouse == null || !sourceWarehouse.IsActive)
        {
            return Result<StockTransferDto>.Fail("Kaynak depo bulunamadý veya aktif deðil.", 400);
        }

        var destWarehouse = await _warehouseRepository.GetByIdAsync(dto.DestinationWarehouseId, cancellationToken);
        if (destWarehouse == null || !destWarehouse.IsActive)
        {
            return Result<StockTransferDto>.Fail("Hedef depo bulunamadý veya aktif deðil.", 400);
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return Result<StockTransferDto>.Fail("Transfer en az bir kalem içermelidir.", 400);
        }

        var productIds = dto.Items.Select(x => x.ProductId).ToList();
        if (productIds.Distinct().Count() != productIds.Count)
        {
            return Result<StockTransferDto>.Fail("Ayný ürün birden fazla kalemde gönderilemez.", 400);
        }

        var stockTransfer = new StockTransfer
        {
            TransferNumber = await _repository.GenerateTransferNumberAsync(cancellationToken),
            SourceWarehouseId = dto.SourceWarehouseId,
            DestinationWarehouseId = dto.DestinationWarehouseId,
            TransferDate = dto.TransferDate ?? DateTime.UtcNow,
            Description = dto.Description,
            Status = StockTransferStatus.Draft,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        foreach (var itemDto in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId, cancellationToken);
            if (product == null)
            {
                return Result<StockTransferDto>.Fail("Silinmiþ veya pasif ürün kullanýlamaz. (ProductId: " + itemDto.ProductId + ")", 400);
            }

            if (itemDto.Quantity <= 0)
            {
                return Result<StockTransferDto>.Fail("Miktar sýfýrdan büyük olmalýdýr.", 400);
            }

            stockTransfer.Items.Add(new StockTransferItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        await _repository.AddAsync(stockTransfer, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var resultDto = await _repository.GetByIdAsync(stockTransfer.Id, cancellationToken);
        return Result<StockTransferDto>.Created(resultDto, "Transfer taslak olarak oluþturuldu.");
    }
}
