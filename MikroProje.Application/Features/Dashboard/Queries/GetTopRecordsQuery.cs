using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Dashboard.DTOs;

namespace MikroProje.Application.Features.Dashboard.Queries;

public class GetTopRecordsQuery : IRequest<Result<DashboardTopRecordsDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
