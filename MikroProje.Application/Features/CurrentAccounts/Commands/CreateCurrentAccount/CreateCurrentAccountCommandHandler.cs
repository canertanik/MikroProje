using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Domain.Entities;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.CurrentAccounts.Commands.CreateCurrentAccount;

public class CreateCurrentAccountCommandHandler : IRequestHandler<CreateCurrentAccountCommand, Result<CurrentAccountDto>>
{
    private readonly ICurrentAccountRepository _repository;
    private readonly IMapper _mapper;

    public CreateCurrentAccountCommandHandler(ICurrentAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<CurrentAccountDto>> Handle(CreateCurrentAccountCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _repository.CodeExistsAsync(request.Code, null, cancellationToken);

        if (codeExists)
        {
            return Result<CurrentAccountDto>.Fail("Current account code already exists.", 409);
        }

        var currentAccount = new CurrentAccount
        {
            Code = request.Code,
            Name = request.Name,
            TaxNumber = request.TaxNumber,
            Phone = request.Phone,
            Email = request.Email,
            Type = request.Type,
            Balance = 0m,
            CreatedDate = DateTime.UtcNow
        };

        await _repository.AddAsync(currentAccount, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<CurrentAccountDto>(currentAccount);
        return Result<CurrentAccountDto>.Created(dto, "Current account created successfully.");
    }
}
