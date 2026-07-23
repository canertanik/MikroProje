using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.SupplierPayments.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.SupplierPayments.Queries.GetSupplierPaymentById;

public class GetSupplierPaymentByIdQueryHandler : IRequestHandler<GetSupplierPaymentByIdQuery, Result<SupplierPaymentDto>>
{
    private readonly ISupplierPaymentRepository _supplierPaymentRepository;
    private readonly IMapper _mapper;

    public GetSupplierPaymentByIdQueryHandler(ISupplierPaymentRepository supplierPaymentRepository, IMapper mapper)
    {
        _supplierPaymentRepository = supplierPaymentRepository;
        _mapper = mapper;
    }

    public async Task<Result<SupplierPaymentDto>> Handle(GetSupplierPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var supplierPayment = await _supplierPaymentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (supplierPayment is null)
        {
            return Result<SupplierPaymentDto>.Fail($"Tedarikçi ödemesi (Id={request.Id}) bulunamadı.", 404);
        }

        var dto = _mapper.Map<SupplierPaymentDto>(supplierPayment);
        return Result<SupplierPaymentDto>.Ok(dto);
    }
}
