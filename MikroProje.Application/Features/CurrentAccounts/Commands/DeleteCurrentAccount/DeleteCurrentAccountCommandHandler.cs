using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.CurrentAccounts.Commands.DeleteCurrentAccount;

public class DeleteCurrentAccountCommandHandler : IRequestHandler<DeleteCurrentAccountCommand, Result<bool>>
{
    private readonly ICacheService _cacheService;
    private readonly ICurrentAccountRepository _repository;

    public DeleteCurrentAccountCommandHandler(ICurrentAccountRepository repository, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteCurrentAccountCommand request, CancellationToken cancellationToken)
    {
        var currentAccount = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (currentAccount is null)
        {
            return Result<bool>.Fail("Current account not found.", 404);
        }

        if (currentAccount.Balance != 0)
        {
            return Result<bool>.Fail("Bakiyesi bulunan cari hesaplar silinemez. Lütfen önce tahsilat veya ödeme ekleyerek bakiyeyi sıfırlayın.", 400);
        }

        currentAccount.IsDeleted = true;
        currentAccount.UpdatedDate = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return Result<bool>.NoContent();
    }
}
