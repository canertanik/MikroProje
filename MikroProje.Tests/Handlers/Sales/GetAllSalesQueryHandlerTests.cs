using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Sales.Queries.GetAllSales;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Sales;

public class GetAllSalesQueryHandlerTests : TestBase
{
    private readonly Mock<ISaleRepository> _repositoryMock;
    private readonly GetAllSalesQueryHandler _handler;

    public GetAllSalesQueryHandlerTests()
    {
        _repositoryMock = new Mock<ISaleRepository>();
        _handler = new GetAllSalesQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var sales = new List<Sale>
        {
            new Sale { Id = 1 },
            new Sale { Id = 2 }
        };
        var totalCount = 2;

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<string?>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((sales, totalCount));

        var query = new GetAllSalesQuery { PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}

