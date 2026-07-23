using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.CurrentAccounts.Commands.UpdateCurrentAccount;

public class UpdateCurrentAccountCommandHandler : IRequestHandler<UpdateCurrentAccountCommand, Result<CurrentAccountDto>>
{
    private readonly ICurrentAccountRepository _repository;
    private readonly IMapper _mapper;

    public UpdateCurrentAccountCommandHandler(ICurrentAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<CurrentAccountDto>> Handle(UpdateCurrentAccountCommand request, CancellationToken cancellationToken)
    {
        var currentAccount = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (currentAccount is null)
        {
            return Result<CurrentAccountDto>.Fail("Current account not found.", 404);
        }

        var codeExists = await _repository.CodeExistsAsync(request.Code, request.Id, cancellationToken);

        if (codeExists)
        {
            return Result<CurrentAccountDto>.Fail("Current account code already exists.", 409);
        }

        currentAccount.Code = request.Code;
        currentAccount.Name = request.Name;
        currentAccount.TaxNumber = request.TaxNumber;
        currentAccount.Phone = request.Phone;
        currentAccount.Email = request.Email;
        currentAccount.Type = request.Type;
        currentAccount.UpdatedDate = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<CurrentAccountDto>(currentAccount);
        return Result<CurrentAccountDto>.Ok(dto, "Current account updated successfully.");
    }
}
