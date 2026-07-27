using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Warehouses.Commands.DeleteWarehouse;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Warehouses;

public class DeleteWarehouseCommandHandlerTests : TestBase
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly DeleteWarehouseCommandHandler _handler;

    public DeleteWarehouseCommandHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _handler = new DeleteWarehouseCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenWarehouseCanBeDeleted()
    {
        var command = new DeleteWarehouseCommand { Id = 1 };
        var warehouse = new Warehouse { Id = 1, IsDefault = false };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _repositoryMock.Setup(r => r.HasRelatedRecordsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(204);
        warehouse.IsDeleted.Should().BeTrue();
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenWarehouseNotFound()
    {
        var command = new DeleteWarehouseCommand { Id = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamad");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenWarehouseIsDefault()
    {
        var command = new DeleteWarehouseCommand { Id = 1 };
        var warehouse = new Warehouse { Id = 1, IsDefault = true };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Varsay");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenWarehouseHasRelatedRecords()
    {
        var command = new DeleteWarehouseCommand { Id = 1 };
        var warehouse = new Warehouse { Id = 1, IsDefault = false };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);
        _repositoryMock.Setup(r => r.HasRelatedRecordsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("kayıtları bulunmaktadır");
    }
}
