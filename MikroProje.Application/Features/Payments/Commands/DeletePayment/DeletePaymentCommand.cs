using MediatR;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Payments.Commands.DeletePayment;

public class DeletePaymentCommand : IRequest<Result<int>>
{
    public int Id { get; set; }
}
