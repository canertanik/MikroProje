using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Warehouses.Commands.SetDefaultWarehouse;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Warehouses;

public class SetDefaultWarehouseCommandHandlerTests : TestBase
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly SetDefaultWarehouseCommandHandler _handler;

    public SetDefaultWarehouseCommandHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _handler = new SetDefaultWarehouseCommandHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenWarehouseCanBeSetAsDefault()
    {
        var command = new SetDefaultWarehouseCommand { Id = 2 };
        var warehouse = new Warehouse { Id = 2, IsActive = true, IsDefault = false };
        var currentDefault = new Warehouse { Id = 1, IsDefault = true };

        _repositoryMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _repositoryMock.Setup(r => r.GetDefaultWarehouseAsync(It.IsAny<CancellationToken>())).ReturnsAsync(currentDefault);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        warehouse.IsDefault.Should().BeTrue();
        currentDefault.IsDefault.Should().BeFalse();
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenWarehouseNotFound()
    {
        var command = new SetDefaultWarehouseCommand { Id = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamad");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenWarehouseIsPassive()
    {
        var command = new SetDefaultWarehouseCommand { Id = 1 };
        var warehouse = new Warehouse { Id = 1, IsActive = false };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Pasif bir depo");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenWarehouseIsAlreadyDefault()
    {
        var command = new SetDefaultWarehouseCommand { Id = 1 };
        var warehouse = new Warehouse { Id = 1, IsActive = true, IsDefault = true };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("zaten");
    }
}
