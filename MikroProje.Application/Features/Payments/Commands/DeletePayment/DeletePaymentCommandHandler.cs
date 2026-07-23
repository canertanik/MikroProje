using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Payments.Commands.DeletePayment;

public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, Result<int>>
{
    private readonly IPaymentRepository _paymentRepository;

    public DeletePaymentCommandHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<int>> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (payment is null)
        {
            return Result<int>.Fail($"Tahsilat/Ödeme (Id={request.Id}) bulunamadı.", 404);
        }

        await _paymentRepository.DeleteAsync(payment, cancellationToken);

        return Result<int>.NoContent("İşlem başarıyla silindi (Soft Delete).");
    }
}
