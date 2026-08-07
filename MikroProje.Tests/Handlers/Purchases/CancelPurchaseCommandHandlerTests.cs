using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Features.Purchases.Commands.CancelPurchase;
using MikroProje.Application.Interfaces;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Purchases;

public class CancelPurchaseCommandHandlerTests : TestBase
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IPurchaseRepository> _purchaseRepoMock;
    private readonly CancelPurchaseCommandHandler _handler;

    public CancelPurchaseCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _purchaseRepoMock = new Mock<IPurchaseRepository>();
        
        _handler = new CancelPurchaseCommandHandler(
            _purchaseRepoMock.Object, 
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPurchaseIsCancelled()
    {
        var command = new CancelPurchaseCommand { Id = 1 };

        _purchaseRepoMock.Setup(x => x.CancelPurchaseAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        
        _cacheServiceMock.Verify(x => x.RemoveByPrefixAsync(CacheKeys.DashboardPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturn404_WhenPurchaseNotFound()
    {
        var command = new CancelPurchaseCommand { Id = 1 };
        
        _purchaseRepoMock.Setup(x => x.CancelPurchaseAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("bulunamadı"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturn400_WhenPurchaseAlreadyReceived()
    {
        var command = new CancelPurchaseCommand { Id = 1 };
        
        _purchaseRepoMock.Setup(x => x.CancelPurchaseAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("iptal edilemez"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("iptal edilemez");
    }
}
