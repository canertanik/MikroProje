using FluentAssertions;
using Moq;
using MikroProje.Application.Features.StockTransfers.Commands.CreateStockTransfer;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;
using MikroProje.Application.Features.StockTransfers.DTOs;

namespace MikroProje.Tests.Handlers.StockTransfers;

public class CreateStockTransferCommandHandlerTests : TestBase
{
    private readonly Mock<IStockTransferRepository> _stockTransferRepositoryMock;
    private readonly Mock<IWarehouseRepository> _warehouseRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly CreateStockTransferCommandHandler _handler;

    public CreateStockTransferCommandHandlerTests()
    {
        _stockTransferRepositoryMock = new Mock<IStockTransferRepository>();
        _warehouseRepositoryMock = new Mock<IWarehouseRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        
        _handler = new CreateStockTransferCommandHandler(
            _stockTransferRepositoryMock.Object, 
            _warehouseRepositoryMock.Object, 
            _productRepositoryMock.Object, 
            Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var sourceWarehouse = new Warehouse { Id = 1, IsActive = true };
        var destWarehouse = new Warehouse { Id = 2, IsActive = true };
        _warehouseRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sourceWarehouse);
        _warehouseRepositoryMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(destWarehouse);

        var product = new Product { Id = 1 };
        _productRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        _stockTransferRepositoryMock.Setup(r => r.GenerateTransferNumberAsync(It.IsAny<CancellationToken>())).ReturnsAsync("TR-001");
        
        var dto = new StockTransferDto { Id = 1, TransferNumber = "TR-001" };
        _stockTransferRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var command = new CreateStockTransferCommand
        {
            Dto = new CreateStockTransferRequestDto
            {
                SourceWarehouseId = 1,
                DestinationWarehouseId = 2,
                Items = new List<CreateStockTransferItemRequestDto> { new CreateStockTransferItemRequestDto { ProductId = 1, Quantity = 10 } }
            }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data.TransferNumber.Should().Be("TR-001");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenSameWarehouse()
    {
        var command = new CreateStockTransferCommand
        {
            Dto = new CreateStockTransferRequestDto { SourceWarehouseId = 1, DestinationWarehouseId = 1 }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("aynı olamaz");
    }
}

