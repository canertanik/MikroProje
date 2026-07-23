using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Payments.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Payments.Queries.GetPaymentById;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository, IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (payment is null)
        {
            return Result<PaymentDto>.Fail($"Tahsilat/Ödeme (Id={request.Id}) bulunamadı.", 404);
        }

        var dto = _mapper.Map<PaymentDto>(payment);
        return Result<PaymentDto>.Ok(dto);
    }
}
