using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.GetAllCurrentAccounts;

public class GetAllCurrentAccountsQueryHandler : IRequestHandler<GetAllCurrentAccountsQuery, Result<IReadOnlyCollection<CurrentAccountDto>>>
{
    private readonly ICurrentAccountRepository _repository;
    private readonly IMapper _mapper;

    public GetAllCurrentAccountsQueryHandler(ICurrentAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyCollection<CurrentAccountDto>>> Handle(GetAllCurrentAccountsQuery request, CancellationToken cancellationToken)
    {
        var currentAccounts = await _repository.GetAllAsync(cancellationToken);
        var currentAccountDtos = _mapper.Map<IReadOnlyCollection<CurrentAccountDto>>(currentAccounts);

        return Result<IReadOnlyCollection<CurrentAccountDto>>.Ok(currentAccountDtos, "Current accounts listed successfully.");
    }
}
