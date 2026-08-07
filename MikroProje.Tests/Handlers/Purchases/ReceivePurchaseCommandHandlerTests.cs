using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.Purchases.Commands.ReceivePurchase;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;
using MikroProje.Domain.Enums;

namespace MikroProje.Tests.Handlers.Purchases;

public class ReceivePurchaseCommandHandlerTests : TestBase
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IPurchaseRepository> _purchaseRepoMock;
    private readonly ReceivePurchaseCommandHandler _handler;

    public ReceivePurchaseCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _purchaseRepoMock = new Mock<IPurchaseRepository>();
        
        _handler = new ReceivePurchaseCommandHandler(
            _purchaseRepoMock.Object, 
            _cacheServiceMock.Object,
            Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPurchaseIsReceived()
    {
        var command = new ReceivePurchaseCommand { Id = 1 };
        var purchase = new Purchase 
        { 
            Id = 1, 
            Status = PurchaseStatus.Received, 
            CurrentAccount = new CurrentAccount { Id = 1, Name = "Tedarikçi" },
            Warehouse = new Warehouse { Id = 1, Name = "Depo" }
        };

        _purchaseRepoMock.Setup(x => x.ReceivePurchaseAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchase);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        
        _cacheServiceMock.Verify(x => x.RemoveByPrefixAsync(CacheKeys.DashboardPrefix, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByPrefixAsync(CacheKeys.ProductsPrefix, It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByPrefixAsync(CacheKeys.CurrentAccountsPrefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturn404_WhenPurchaseNotFound()
    {
        var command = new ReceivePurchaseCommand { Id = 1 };
        
        _purchaseRepoMock.Setup(x => x.ReceivePurchaseAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("bulunamadı"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturn409_WhenPurchaseAlreadyReceived()
    {
        var command = new ReceivePurchaseCommand { Id = 1 };
        
        _purchaseRepoMock.Setup(x => x.ReceivePurchaseAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("zaten depoya alınmış"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("zaten depoya alınmış");
    }
}
