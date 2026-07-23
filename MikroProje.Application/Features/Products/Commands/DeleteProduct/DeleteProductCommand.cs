using MediatR;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
