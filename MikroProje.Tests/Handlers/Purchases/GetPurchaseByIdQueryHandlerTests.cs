using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Purchases.Queries.GetPurchaseById;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Purchases;

public class GetPurchaseByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IPurchaseRepository> _repositoryMock;
    private readonly GetPurchaseByIdQueryHandler _handler;

    public GetPurchaseByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IPurchaseRepository>();
        _handler = new GetPurchaseByIdQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPurchaseExists()
    {
        var purchase = new Purchase { Id = 1 };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(purchase);

        var query = new GetPurchaseByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenPurchaseNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Purchase?)null);

        var query = new GetPurchaseByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamad");
    }
}

