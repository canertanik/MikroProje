using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Payments.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.Payments.Commands.CreatePayment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    private readonly ICacheService _cacheService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICurrentAccountRepository _currentAccountRepository;
    private readonly IMapper _mapper;

    public CreatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ICurrentAccountRepository currentAccountRepository,
        IMapper mapper, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _paymentRepository = paymentRepository;
        _currentAccountRepository = currentAccountRepository;
        _mapper = mapper;
    }

    public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var currentAccount = await _currentAccountRepository.GetByIdAsync(request.CurrentAccountId, cancellationToken);
        if (currentAccount is null)
        {
            return Result<PaymentDto>.Fail($"Cari hesap (Id={request.CurrentAccountId}) bulunamadı.", 404);
        }

        if (currentAccount.Type != CurrentAccountType.Customer && currentAccount.Type != CurrentAccountType.Both)
        {
            return Result<PaymentDto>.Fail("Tahsilat işlemi yalnızca müşteri (Customer) veya Both türündeki cariler için yapılabilir.", 400);
        }

        if (request.Type == PaymentType.Collection && request.Amount > currentAccount.Balance)
        {
            return Result<PaymentDto>.Fail($"Tahsilat tutarı ({request.Amount:N2} TL) müşterinin mevcut borcundan ({currentAccount.Balance:N2} TL) büyük olamaz.", 400);
        }

        var payment = new Payment
        {
            CurrentAccountId = request.CurrentAccountId,
            Amount = request.Amount,
            Type = request.Type,
            PaymentMethod = request.PaymentMethod,
            Description = request.Description,
            PaymentDate = request.PaymentDate,
            CreatedDate = DateTime.UtcNow
        };

        // Bakiye düşme işlemi repository içindeki transaction'da yapılacak (Amount kadar)
        // İster Collection (Tahsilat) ister Payment (Ödeme) olsun, Amount kadar işlem görecek
        // Not: Görevde "Tahsilat oluşturulduğunda CurrentAccount.Balance alanından Amount kadar düş" deniyor.
        // Bu yüzden bakiyeden Amount'u çıkaracağız. Eğer "Ödeme (Payment)" olursa bakiye artmalı mı?
        // İster Collection ister Payment olsun, soruda "Amount kadar düş" denmiş (muhtemelen tahsilat yapınca cari borcu azalır mantığı).
        // Bu yüzden transaction içinde bakiyeyi düşeceğiz.

        var createdPayment = await _paymentRepository.CreateAsync(payment, currentAccount, cancellationToken);

        var dto = _mapper.Map<PaymentDto>(createdPayment);
        return Result<PaymentDto>.Created(dto, "Tahsilat başarıyla oluşturuldu.");
    }
}
