using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Payments.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Payments.Queries.GetPayments;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, Result<PagedResult<PaymentDto>>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public GetPaymentsQueryHandler(IPaymentRepository paymentRepository, IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _paymentRepository.GetAllAsync(
            request.CurrentAccountId, request.PageNumber, request.PageSize, cancellationToken);

        var dtos = _mapper.Map<IReadOnlyCollection<PaymentDto>>(items);
        var pagedResult = PagedResult<PaymentDto>.Create(dtos, request.PageNumber, request.PageSize, totalCount);

        return Result<PagedResult<PaymentDto>>.Ok(pagedResult);
    }
}
