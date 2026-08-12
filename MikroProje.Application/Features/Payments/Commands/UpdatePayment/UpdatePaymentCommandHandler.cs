using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Payments.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.Payments.Commands.UpdatePayment;

public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Result<PaymentDto>>
{
    private readonly ICacheService _cacheService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICurrentAccountRepository _currentAccountRepository;
    private readonly IMapper _mapper;

    public UpdatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ICurrentAccountRepository currentAccountRepository,
        IMapper mapper,
        ICacheService cacheService)
    {
        _cacheService = cacheService;
        _paymentRepository = paymentRepository;
        _currentAccountRepository = currentAccountRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaymentDto>> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (payment is null)
        {
            return Result<PaymentDto>.Fail($"Tahsilat/Ödeme (Id={request.Id}) bulunamadı.", 404);
        }

        var targetAccount = await _currentAccountRepository.GetByIdAsync(request.CurrentAccountId, cancellationToken);
        if (targetAccount is null)
        {
            return Result<PaymentDto>.Fail($"Cari hesap (Id={request.CurrentAccountId}) bulunamadı.", 404);
        }

        if (targetAccount.Type != CurrentAccountType.Customer && targetAccount.Type != CurrentAccountType.Both)
        {
            return Result<PaymentDto>.Fail("Tahsilat yalnızca müşteri veya her iki türdeki cariler için güncellenebilir.", 400);
        }

        var availableBalance = targetAccount.Balance;
        if (payment.CurrentAccountId == targetAccount.Id)
        {
            availableBalance += payment.Amount;
        }

        if (request.Amount > availableBalance)
        {
            return Result<PaymentDto>.Fail(
                $"Tahsilat tutarı ({request.Amount:N2} TL) müşterinin mevcut borcundan ({availableBalance:N2} TL) büyük olamaz.",
                400);
        }

        var originalAccount = payment.CurrentAccount;
        var originalAmount = payment.Amount;

        payment.CurrentAccountId = targetAccount.Id;
        payment.Amount = request.Amount;
        payment.PaymentMethod = request.PaymentMethod;
        payment.Description = request.Description;
        payment.PaymentDate = request.PaymentDate;

        var updatedPayment = await _paymentRepository.UpdateAsync(
            payment,
            originalAccount,
            targetAccount,
            originalAmount,
            request.RowVersion,
            cancellationToken);
        var dto = _mapper.Map<PaymentDto>(updatedPayment);

        return Result<PaymentDto>.Ok(dto, "İşlem başarıyla güncellendi.");
    }
}
