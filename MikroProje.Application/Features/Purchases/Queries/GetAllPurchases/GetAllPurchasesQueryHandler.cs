using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Purchases.Queries.GetAllPurchases;

public class GetAllPurchasesQueryHandler : IRequestHandler<GetAllPurchasesQuery, Result<PagedResult<PurchaseListDto>>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly IMapper _mapper;

    public GetAllPurchasesQueryHandler(IPurchaseRepository purchaseRepository, IMapper mapper)
    {
        _purchaseRepository = purchaseRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PurchaseListDto>>> Handle(GetAllPurchasesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _purchaseRepository.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);
        
        var dtoList = _mapper.Map<List<PurchaseListDto>>(items);
        var pagedResult = PagedResult<PurchaseListDto>.Create(dtoList, request.PageNumber, request.PageSize, totalCount);

        return Result<PagedResult<PurchaseListDto>>.Ok(pagedResult);
    }
}
