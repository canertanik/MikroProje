using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.Sales.Commands.CreateSale;
using MikroProje.Application.Features.Sales.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Sales;

public class CreateSaleCommandHandlerTests : TestBase
{
    private readonly Mock<ISaleRepository> _saleRepoMock;
    private readonly Mock<ICurrentAccountRepository> _accountRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly CreateSaleCommandHandler _handler;

    public CreateSaleCommandHandlerTests()
    {
        _saleRepoMock = new Mock<ISaleRepository>();
        _accountRepoMock = new Mock<ICurrentAccountRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        
        _handler = new CreateSaleCommandHandler(
            _saleRepoMock.Object, 
            _accountRepoMock.Object, 
            _productRepoMock.Object, 
            Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            CurrentAccountId = 1,
            Items = new List<SaleItemDto>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 50, Discount = 0 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId };
        var product = new Product { Id = command.Items[0].ProductId, StockQuantity = 10, SalePrice = 50, VatRate = 18 };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _productRepoMock.Setup(r => r.GetByIdAsync(command.Items[0].ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _saleRepoMock.Setup(r => r.CreateSaleAsync(It.IsAny<Sale>(), It.IsAny<List<SaleLineItem>>(), It.IsAny<CurrentAccount>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale s, List<SaleLineItem> items, CurrentAccount a, CancellationToken ct) => s);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        
        _saleRepoMock.Verify(r => r.CreateSaleAsync(It.IsAny<Sale>(), It.IsAny<List<SaleLineItem>>(), It.IsAny<CurrentAccount>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenAccountNotFound()
    {
        // Arrange
        var command = new CreateSaleCommand { CurrentAccountId = 1, Items = new List<SaleItemDto>() };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrentAccount?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunama");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenProductNotFound()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            CurrentAccountId = 1,
            Items = new List<SaleItemDto>
            {
                new() { ProductId = 1, Quantity = 2 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId };
        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _productRepoMock.Setup(r => r.GetByIdAsync(command.Items[0].ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunama");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenStockIsInsufficient()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            CurrentAccountId = 1,
            Items = new List<SaleItemDto>
            {
                new() { ProductId = 1, Quantity = 5 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId };
        var product = new Product { Id = command.Items[0].ProductId, StockQuantity = 2 }; // Less than requested

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _productRepoMock.Setup(r => r.GetByIdAsync(command.Items[0].ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(422);
        result.Message.Should().Contain("yetersiz stok");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            CurrentAccountId = 1,
            Items = new List<SaleItemDto>
            {
                new() { ProductId = 1, Quantity = 1 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId };
        var product = new Product { Id = command.Items[0].ProductId, StockQuantity = 10 };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _productRepoMock.Setup(r => r.GetByIdAsync(command.Items[0].ProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _saleRepoMock.Setup(r => r.CreateSaleAsync(It.IsAny<Sale>(), It.IsAny<List<SaleLineItem>>(), It.IsAny<CurrentAccount>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("conflict"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("zaman");
    }
}


