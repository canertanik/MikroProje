using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.SupplierPayments.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.SupplierPayments.Commands.CreateSupplierPayment;

public class CreateSupplierPaymentCommandHandler : IRequestHandler<CreateSupplierPaymentCommand, Result<SupplierPaymentDto>>
{
    private readonly ICacheService _cacheService;
    private readonly ISupplierPaymentRepository _supplierPaymentRepository;
    private readonly ICurrentAccountRepository _currentAccountRepository;
    private readonly IMapper _mapper;

    public CreateSupplierPaymentCommandHandler(
        ISupplierPaymentRepository supplierPaymentRepository,
        ICurrentAccountRepository currentAccountRepository,
        IMapper mapper, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _supplierPaymentRepository = supplierPaymentRepository;
        _currentAccountRepository = currentAccountRepository;
        _mapper = mapper;
    }

    public async Task<Result<SupplierPaymentDto>> Handle(CreateSupplierPaymentCommand request, CancellationToken cancellationToken)
    {
        var currentAccount = await _currentAccountRepository.GetByIdAsync(request.CurrentAccountId, cancellationToken);
        if (currentAccount is null)
        {
            return Result<SupplierPaymentDto>.Fail($"Cari hesap (Id={request.CurrentAccountId}) bulunamadı.", 404);
        }

        if (currentAccount.Type != CurrentAccountType.Supplier)
        {
            return Result<SupplierPaymentDto>.Fail("Ödeme işlemi yalnızca tedarikçi (Supplier) türündeki cariler için yapılabilir.", 400);
        }

        if (request.Amount > currentAccount.Balance)
        {
            return Result<SupplierPaymentDto>.Fail("Ödeme tutarı mevcut borçtan (bakiyeden) büyük olamaz.", 400);
        }

        var supplierPayment = new SupplierPayment
        {
            CurrentAccountId = request.CurrentAccountId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            ReferenceNumber = request.ReferenceNumber,
            Description = request.Description,
            PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        var createdPayment = await _supplierPaymentRepository.CreateAsync(supplierPayment, currentAccount, cancellationToken);

        var dto = _mapper.Map<SupplierPaymentDto>(createdPayment);
        return Result<SupplierPaymentDto>.Created(dto, "Tedarikçi ödemesi başarıyla oluşturuldu.");
    }
}
