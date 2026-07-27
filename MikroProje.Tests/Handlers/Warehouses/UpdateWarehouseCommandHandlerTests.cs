using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.Warehouses.Commands.UpdateWarehouse;
using MikroProje.Application.Features.Warehouses.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Warehouses;

public class UpdateWarehouseCommandHandlerTests : TestBase
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly UpdateWarehouseCommandHandler _handler;

    public UpdateWarehouseCommandHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _handler = new UpdateWarehouseCommandHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var commandId = 1;
        var command = new UpdateWarehouseCommand
        {
            Id = commandId,
            Dto = new UpdateWarehouseRequestDto
            {
                Id = commandId,
                Code = "WH-02",
                Name = "Updated Warehouse",
                IsActive = true
            }
        };

        var existingWarehouse = new Warehouse { Id = command.Id, Code = "WH-01", IsActive = true };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingWarehouse);
            
        _repositoryMock.Setup(r => r.CodeExistsAsync(It.IsAny<string>(), command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Warehouse>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse w, byte[] v, CancellationToken ct) => w);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(command.Dto.Code);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenIdMismatch()
    {
        // Arrange
        var command = new UpdateWarehouseCommand
        {
            Id = 1, Dto = new UpdateWarehouseRequestDto { Id = 2 }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("ID uyu");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenEntityNotFound()
    {
        // Arrange
        var commandId = 1;
        var command = new UpdateWarehouseCommand
        {
            Id = commandId,
            Dto = new UpdateWarehouseRequestDto { Id = commandId }
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

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
        var commandId = 1;
        var command = new UpdateWarehouseCommand
        {
            Id = commandId,
            Dto = new UpdateWarehouseRequestDto { Id = commandId, Code = "WH-01", Name = "WH", IsActive = true }
        };

        var existingWarehouse = new Warehouse { Id = command.Id, Code = "WH-01", IsActive = true };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingWarehouse);
            
        _repositoryMock.Setup(r => r.CodeExistsAsync(It.IsAny<string>(), command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Warehouse>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("conflict"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("kullan");
    }
}






