using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.Sales.Queries.GetSalesByCurrentAccount;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Sales;

public class GetSalesByCurrentAccountQueryHandlerTests : TestBase
{
    private readonly Mock<ISaleRepository> _saleRepositoryMock;
    private readonly GetSalesByCurrentAccountQueryHandler _handler;

    public GetSalesByCurrentAccountQueryHandlerTests()
    {
        _saleRepositoryMock = new Mock<ISaleRepository>();
        _handler = new GetSalesByCurrentAccountQueryHandler(_saleRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var sales = new List<Sale>
        {
            new Sale { Id = 1, CurrentAccountId = 1, TotalAmount = 100 },
            new Sale { Id = 2, CurrentAccountId = 1, TotalAmount = 200 }
        };

        _saleRepositoryMock.Setup(r => r.GetByCurrentAccountAsync(1, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((sales, 2));

        var query = new GetSalesByCurrentAccountQuery { CurrentAccountId = 1, PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}
