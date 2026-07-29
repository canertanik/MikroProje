using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.StockMovements.Commands.CreateStockMovement;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.StockMovements;

public class CreateStockMovementCommandHandlerTests : TestBase
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly CreateStockMovementCommandHandler _handler;

    public CreateStockMovementCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new CreateStockMovementCommandHandler(_stockMovementRepositoryMock.Object, _productRepositoryMock.Object, Mapper, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var product = new Product { Id = 1, StockQuantity = 10 };
        _productRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var movement = new StockMovement { Id = 1, ProductId = 1, Quantity = 5, MovementType = StockMovementType.StockIn };
        _stockMovementRepositoryMock.Setup(r => r.CreateAsync(
            1, StockMovementType.StockIn, It.IsAny<StockMovementSourceType>(), 5, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(movement);

        var command = new CreateStockMovementCommand { ProductId = 1, Quantity = 5, MovementType = StockMovementType.StockIn };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenProductNotFound()
    {
        _productRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var command = new CreateStockMovementCommand { ProductId = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenInsufficientStock()
    {
        var product = new Product { Id = 1, StockQuantity = 10 };
        _productRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        _stockMovementRepositoryMock.Setup(r => r.CreateAsync(
            1, StockMovementType.StockOut, It.IsAny<StockMovementSourceType>(), 15, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Yetersiz stok"));

        var command = new CreateStockMovementCommand { ProductId = 1, Quantity = 15, MovementType = StockMovementType.StockOut };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Yetersiz stok");
    }
}

