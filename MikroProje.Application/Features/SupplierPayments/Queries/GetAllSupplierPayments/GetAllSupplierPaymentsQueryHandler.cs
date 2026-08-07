using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.SupplierPayments.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.SupplierPayments.Queries.GetAllSupplierPayments;

public class GetAllSupplierPaymentsQueryHandler : IRequestHandler<GetAllSupplierPaymentsQuery, Result<PagedResult<SupplierPaymentListDto>>>
{
    private readonly ISupplierPaymentRepository _supplierPaymentRepository;
    private readonly IMapper _mapper;

    public GetAllSupplierPaymentsQueryHandler(ISupplierPaymentRepository supplierPaymentRepository, IMapper mapper)
    {
        _supplierPaymentRepository = supplierPaymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<SupplierPaymentListDto>>> Handle(GetAllSupplierPaymentsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _supplierPaymentRepository.GetAllAsync(
            request.CurrentAccountId,
            request.SearchTerm,
            request.StartDate,
            request.EndDate,
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);

        var dtos = _mapper.Map<IReadOnlyCollection<SupplierPaymentListDto>>(items);

        var pagedResult = PagedResult<SupplierPaymentListDto>.Create(
            dtos, request.PageNumber, request.PageSize, totalCount);

        return Result<PagedResult<SupplierPaymentListDto>>.Ok(pagedResult);
    }
}
