using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.AI.Queries.GetProductForecast;

public class GetProductForecastQuery : IRequest<Result<ForecastResultDto>>
{
    public int ProductId { get; set; }
}

public class GetProductForecastQueryHandler : IRequestHandler<GetProductForecastQuery, Result<ForecastResultDto>>
{
    private readonly IDemandForecastService _forecastService;

    public GetProductForecastQueryHandler(IDemandForecastService forecastService)
    {
        _forecastService = forecastService;
    }

    public async Task<Result<ForecastResultDto>> Handle(GetProductForecastQuery request, CancellationToken cancellationToken)
    {
        return await _forecastService.GetProductForecastAsync(request.ProductId, cancellationToken);
    }
}
