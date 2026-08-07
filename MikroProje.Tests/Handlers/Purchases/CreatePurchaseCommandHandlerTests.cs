using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.Purchases.Commands.CreatePurchase;
using MikroProje.Application.Features.Purchases.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Purchases;

public class CreatePurchaseCommandHandlerTests : TestBase
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IPurchaseRepository> _purchaseRepoMock;
    private readonly Mock<ICurrentAccountRepository> _accountRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepoMock;
    private readonly CreatePurchaseCommandHandler _handler;

    public CreatePurchaseCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _purchaseRepoMock = new Mock<IPurchaseRepository>();
        _accountRepoMock = new Mock<ICurrentAccountRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _warehouseRepoMock = new Mock<IWarehouseRepository>();
        
        _handler = new CreatePurchaseCommandHandler(
            _purchaseRepoMock.Object, 
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
        var command = new CreatePurchaseCommand
        {
            CurrentAccountId = 1,
            WarehouseId = 1,
            Items = new List<CreatePurchaseItemRequest>
            {
                new() { ProductId = 1, Quantity = 5, UnitPrice = 20 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId, Type = CurrentAccountType.Supplier };
        var product = new Product { Id = command.Items[0].ProductId, StockQuantity = 10, PurchasePrice = 20, VatRate = 18 };
        var warehouse = new Warehouse { Id = 1, IsActive = true, Name = "Main" };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _warehouseRepoMock.Setup(r => r.GetByIdAsync(command.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        _purchaseRepoMock.Setup(r => r.CreatePurchaseAsync(It.IsAny<Purchase>(), It.IsAny<List<PurchaseItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Purchase p, List<PurchaseItem> items, CancellationToken ct) => p);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        
        _purchaseRepoMock.Verify(r => r.CreatePurchaseAsync(It.IsAny<Purchase>(), It.IsAny<List<PurchaseItem>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenAccountNotFound()
    {
        // Arrange
        var command = new CreatePurchaseCommand { CurrentAccountId = 1, Items = new List<CreatePurchaseItemRequest>() };

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
    public async Task Handle_ShouldReturnFail_WhenAccountIsNotSupplier()
    {
        // Arrange
        var command = new CreatePurchaseCommand { CurrentAccountId = 1, Items = new List<CreatePurchaseItemRequest>() };
        var account = new CurrentAccount { Id = command.CurrentAccountId, Type = CurrentAccountType.Customer };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Tedarik");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenProductNotFound()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            CurrentAccountId = 1,
            Items = new List<CreatePurchaseItemRequest>
            {
                new() { ProductId = 1, Quantity = 2 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId, Type = CurrentAccountType.Supplier };
        
        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>()); // Empty list means product not found

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunama");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            CurrentAccountId = 1,
            WarehouseId = 1,
            Items = new List<CreatePurchaseItemRequest>
            {
                new() { ProductId = 1, Quantity = 1 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId, Type = CurrentAccountType.Supplier };
        var product = new Product { Id = command.Items[0].ProductId };
        var warehouse = new Warehouse { Id = 1, IsActive = true, Name = "Main" };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
            
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(command.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        _purchaseRepoMock.Setup(r => r.CreatePurchaseAsync(It.IsAny<Purchase>(), It.IsAny<List<PurchaseItem>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("conflict"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("zaman");
    }

    [Fact]
    public async Task Handle_ShouldInvalidateDashboardCache_WhenPurchaseCreatedSuccessfully()
    {
        // Arrange
        var command = new CreatePurchaseCommand
        {
            CurrentAccountId = 1,
            WarehouseId = 1,
            Items = new List<CreatePurchaseItemRequest>
            {
                new() { ProductId = 1, Quantity = 3, UnitPrice = 50 }
            }
        };

        var account = new CurrentAccount { Id = command.CurrentAccountId, Type = CurrentAccountType.Supplier };
        var product = new Product { Id = command.Items[0].ProductId, PurchasePrice = 50, VatRate = 18 };
        var warehouse = new Warehouse { Id = 1, IsActive = true, Name = "Main" };

        _accountRepoMock.Setup(r => r.GetByIdAsync(command.CurrentAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
            
        _warehouseRepoMock.Setup(r => r.GetByIdAsync(command.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);

        _productRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        _purchaseRepoMock.Setup(r => r.CreatePurchaseAsync(It.IsAny<Purchase>(), It.IsAny<List<PurchaseItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Purchase p, List<PurchaseItem> items, CancellationToken ct) => p);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        // Dashboard cache MUST be invalidated after successful purchase
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync(
            CacheKeys.DashboardPrefix,
            It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task Handle_ShouldReturn404_WhenWarehouseNotFound()
    {
        var command = new CreatePurchaseCommand { CurrentAccountId = 1, WarehouseId = 99, Items = new List<CreatePurchaseItemRequest> { new() { ProductId = 1, Quantity = 1 } } };
        var account = new CurrentAccount { Id = 1, Type = CurrentAccountType.Supplier };
        _accountRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _warehouseRepoMock.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);
        var result = await _handler.Handle(command, CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task Handle_ShouldReturn422_WhenWarehouseIsPassive()
    {
        var command = new CreatePurchaseCommand { CurrentAccountId = 1, WarehouseId = 2, Items = new List<CreatePurchaseItemRequest> { new() { ProductId = 1, Quantity = 1 } } };
        var account = new CurrentAccount { Id = 1, Type = CurrentAccountType.Supplier };
        var warehouse = new Warehouse { Id = 2, IsActive = false, Name = "Passive" };
        _accountRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);
        _warehouseRepoMock.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        var result = await _handler.Handle(command, CancellationToken.None);
        Assert.False(result.Success);
        Assert.Equal(422, result.StatusCode);
    }
}

