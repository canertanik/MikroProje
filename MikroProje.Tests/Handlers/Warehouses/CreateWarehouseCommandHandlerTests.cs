using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Warehouses.Commands.CreateWarehouse;
using MikroProje.Application.Features.Warehouses.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Warehouses;

public class CreateWarehouseCommandHandlerTests : TestBase
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly CreateWarehouseCommandHandler _handler;

    public CreateWarehouseCommandHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _handler = new CreateWarehouseCommandHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Dto = new CreateWarehouseRequestDto
            {
                Code = "WH-01",
                Name = "Main Warehouse",
                Description = "Default warehouse",
                IsDefault = true,
                IsActive = true
            }
        };

        _repositoryMock.Setup(r => r.CodeExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _repositoryMock.Setup(r => r.GetDefaultWarehouseAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);
            
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(command.Dto.Code);
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Warehouse>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenCodeAlreadyExists()
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Dto = new CreateWarehouseRequestDto { Code = "WH-01", Name = "Main Warehouse", IsActive = true }
        };

        _repositoryMock.Setup(r => r.CodeExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("kullan");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenPassiveWarehouseSetAsDefault()
    {
        // Arrange
        var command = new CreateWarehouseCommand
        {
            Dto = new CreateWarehouseRequestDto { Code = "WH-01", Name = "Passive WH", IsDefault = true, IsActive = false }
        };

        _repositoryMock.Setup(r => r.CodeExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("varsay");
    }
}



