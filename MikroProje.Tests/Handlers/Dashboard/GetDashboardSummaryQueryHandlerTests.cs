using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Dashboard.Queries;
using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Dashboard;

public class GetDashboardSummaryQueryHandlerTests : TestBase
{
    private readonly Mock<IDashboardRepository> _repositoryMock;
    private readonly GetDashboardSummaryQueryHandler _handler;

    public GetDashboardSummaryQueryHandlerTests()
    {
        _repositoryMock = new Mock<IDashboardRepository>();
        _handler = new GetDashboardSummaryQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithSummaryData()
    {
        var summary = new DashboardSummaryDto
        {
            TotalProductCount = 100,
            TotalCustomerCount = 50,
            TotalStockQuantity = 200,
            ThisMonthSalesTotal = 50000m
        };

        _repositoryMock.Setup(r => r.GetSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var query = new GetDashboardSummaryQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalProductCount.Should().Be(100);
        result.Data.ThisMonthSalesTotal.Should().Be(50000m);
    }
}

