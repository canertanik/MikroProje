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
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ISaleRepository> _saleRepoMock;
    private readonly Mock<ICurrentAccountRepository> _accountRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
    private readonly CreateSaleCommandHandler _handler;

    public CreateSaleCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _saleRepoMock = new Mock<ISaleRepository>();
        _accountRepoMock = new Mock<ICurrentAccountRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _warehouseRepoMock = new Mock<IWarehouseRepository>();
        
        _handler = new CreateSaleCommandHandler(
            _saleRepoMock.Object, 
            _accountRepoMock.Object, 
            _productRepoMock.Object, 
            _warehouseRepoMock.Object,
            Mapper, 
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            CurrentAccountId = 1,
            WarehouseId = 1,
            Items = new List<SaleItemDto>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 50, Discount = 0 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId, Type = MikroProje.Domain.Enums.CurrentAccountType.Customer };
        var product = new Product { Id = command.Items[0].ProductId, StockQuantity = 10, SalePrice = 50, VatRate = 18 };
        var warehouse = new Warehouse { Id = 1, IsActive = true, Name = "Main" };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _warehouseRepoMock.Setup(r => r.GetByIdAsync(command.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

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

        var account = new CurrentAccount { Id = command.CurrentAccountId, Type = MikroProje.Domain.Enums.CurrentAccountType.Customer };
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
            WarehouseId = 1,
            Items = new List<SaleItemDto>
            {
                new() { ProductId = 1, Quantity = 5 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId, Type = MikroProje.Domain.Enums.CurrentAccountType.Customer };
        var product = new Product { Id = command.Items[0].ProductId, StockQuantity = 2 }; // Less than requested
        var warehouse = new Warehouse { Id = 1, IsActive = true, Name = "Main" };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
            
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(command.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

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
            WarehouseId = 1,
            Items = new List<SaleItemDto>
            {
                new() { ProductId = 1, Quantity = 1 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId, Type = MikroProje.Domain.Enums.CurrentAccountType.Customer };
        var product = new Product { Id = command.Items[0].ProductId, StockQuantity = 10 };
        var warehouse = new Warehouse { Id = 1, IsActive = true, Name = "Main" };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
            
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(command.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

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
    [Fact]
    public async Task Handle_ShouldReturn404_WhenWarehouseNotFound()
    {
        var command = new CreateSaleCommand { CurrentAccountId = 1, WarehouseId = 99, Items = new List<SaleItemDto> { new() { ProductId = 1, Quantity = 1 } } };
        var account = new CurrentAccount { Id = 1, Type = MikroProje.Domain.Enums.CurrentAccountType.Customer };
        _accountRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _warehouseRepoMock.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);
        var result = await _handler.Handle(command, CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldReturn422_WhenWarehouseIsPassive()
    {
        var command = new CreateSaleCommand { CurrentAccountId = 1, WarehouseId = 2, Items = new List<SaleItemDto> { new() { ProductId = 1, Quantity = 1 } } };
        var account = new CurrentAccount { Id = 1, Type = MikroProje.Domain.Enums.CurrentAccountType.Customer };
        var warehouse = new Warehouse { Id = 2, IsActive = false, Name = "Passive" };
        _accountRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _warehouseRepoMock.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        var result = await _handler.Handle(command, CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal(422, result.StatusCode);
    }
}

