using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.GetCurrentAccountById;

public class GetCurrentAccountByIdQueryHandler : IRequestHandler<GetCurrentAccountByIdQuery, Result<CurrentAccountDto>>
{
    private readonly ICurrentAccountRepository _repository;
    private readonly IMapper _mapper;

    public GetCurrentAccountByIdQueryHandler(ICurrentAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<CurrentAccountDto>> Handle(GetCurrentAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var currentAccount = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (currentAccount is null)
        {
            return Result<CurrentAccountDto>.Fail("Current account not found.", 404);
        }

        var dto = _mapper.Map<CurrentAccountDto>(currentAccount);
        return Result<CurrentAccountDto>.Ok(dto, "Current account retrieved successfully.");
    }
}
