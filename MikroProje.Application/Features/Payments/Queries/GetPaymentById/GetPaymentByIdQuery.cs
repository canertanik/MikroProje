using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Payments.DTOs;

namespace MikroProje.Application.Features.Payments.Queries.GetPaymentById;

public class GetPaymentByIdQuery : IRequest<Result<PaymentDto>>
{
    public int Id { get; set; }
}
