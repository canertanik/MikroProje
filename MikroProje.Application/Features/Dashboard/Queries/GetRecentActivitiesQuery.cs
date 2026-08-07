using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Dashboard.DTOs;

namespace MikroProje.Application.Features.Dashboard.Queries;

public class GetRecentActivitiesQuery : IRequest<Result<List<DashboardRecentActivityDto>>>
{
}
