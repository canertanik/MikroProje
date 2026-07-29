using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.Products.Commands.UpdateProduct;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Products;

public class UpdateProductCommandHandlerTests : TestBase
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new UpdateProductCommandHandler(_repositoryMock.Object, Mapper, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1, Code = "PRD-002", Barcode = "987654321" };
        var existingProduct = new Product { Id = command.Id, Code = "PRD-001", Barcode = "123456789" };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.BarcodeExistsAsync(command.Barcode, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(command.Code);
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenEntityNotFound()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenCodeAlreadyExists()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1, Code = "PRD-002" };
        var existingProduct = new Product { Id = command.Id };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("code already exists");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenBarcodeAlreadyExists()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1, Barcode = "123" };
        var existingProduct = new Product { Id = command.Id };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.BarcodeExistsAsync(command.Barcode, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("barcode already exists");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        var command = new UpdateProductCommand { Id = 1, Code = "PRD-002", Barcode = "987654321" };
        var existingProduct = new Product { Id = command.Id };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.BarcodeExistsAsync(command.Barcode, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("conflict"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("Concurrent update detected");
    }
}




