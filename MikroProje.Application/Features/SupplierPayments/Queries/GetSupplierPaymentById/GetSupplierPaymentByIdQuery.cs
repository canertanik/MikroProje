using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.SupplierPayments.DTOs;

namespace MikroProje.Application.Features.SupplierPayments.Queries.GetSupplierPaymentById;

public class GetSupplierPaymentByIdQuery : IRequest<Result<SupplierPaymentDto>>
{
    public int Id { get; set; }
}
