using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Warehouses.Commands.DeleteWarehouse;

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, Result<bool>>
{
    private readonly IWarehouseRepository _repository;

    public DeleteWarehouseCommandHandler(IWarehouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result<bool>.Fail("Depo bulunamadý.", 404);
        }

        if (warehouse.IsDefault)
        {
            return Result<bool>.Fail("Varsayýlan depo silinemez. Lütfen önce baþka bir depoyu varsayýlan yapýn.", 400);
        }

        if (await _repository.HasRelatedRecordsAsync(request.Id, cancellationToken))
        {
            return Result<bool>.Fail("Bu depoya ait stok, satýþ veya stok hareketi kayýtlarý bulunmaktadýr. Depoyu silemezsiniz, ancak pasif duruma getirebilirsiniz.", 400);
        }

        warehouse.IsDeleted = true;
        warehouse.UpdatedDate = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        return Result<bool>.NoContent("Depo silindi.");
    }
}
