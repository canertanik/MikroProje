using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Purchases.Queries.GetAllPurchases;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Purchases;

public class GetAllPurchasesQueryHandlerTests : TestBase
{
    private readonly Mock<IPurchaseRepository> _repositoryMock;
    private readonly GetAllPurchasesQueryHandler _handler;

    public GetAllPurchasesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IPurchaseRepository>();
        _handler = new GetAllPurchasesQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var purchases = new List<Purchase>
        {
            new Purchase { Id = 1 },
            new Purchase { Id = 2 }
        };
        var totalCount = 2;

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<MikroProje.Domain.Enums.PurchaseStatus?>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((purchases, totalCount));

        var query = new GetAllPurchasesQuery { PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}

