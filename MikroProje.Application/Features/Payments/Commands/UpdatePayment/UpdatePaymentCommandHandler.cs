using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Payments.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Payments.Commands.UpdatePayment;

public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public UpdatePaymentCommandHandler(IPaymentRepository paymentRepository, IMapper mapper)
    {
        _paymentRepository = paymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaymentDto>> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (payment is null)
        {
            return Result<PaymentDto>.Fail($"Tahsilat/Ödeme (Id={request.Id}) bulunamadı.", 404);
        }

        payment.PaymentMethod = request.PaymentMethod;
        payment.Description = request.Description;

        var updatedPayment = await _paymentRepository.UpdateAsync(payment, request.RowVersion, cancellationToken);
        var dto = _mapper.Map<PaymentDto>(updatedPayment);

        return Result<PaymentDto>.Ok(dto, "İşlem başarıyla güncellendi.");
    }
}
