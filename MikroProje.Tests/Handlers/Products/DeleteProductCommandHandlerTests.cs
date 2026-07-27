using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.Products.Commands.DeleteProduct;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Products;

public class DeleteProductCommandHandlerTests : TestBase
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new DeleteProductCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntityExists()
    {
        // Arrange
        var commandId = 1;
        var command = new DeleteProductCommand { Id = commandId };
        var existingProduct = new Product { Id = commandId, Name = "Laptop" };

        _repositoryMock.Setup(r => r.GetByIdAsync(commandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
            
        _repositoryMock.Setup(r => r.DeleteSoftAsync(existingProduct, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(204);
        
        _repositoryMock.Verify(r => r.DeleteSoftAsync(existingProduct, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenEntityNotFound()
    {
        // Arrange
        var commandId = 1;
        var command = new DeleteProductCommand { Id = commandId };

        _repositoryMock.Setup(r => r.GetByIdAsync(commandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Product not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        var commandId = 1;
        var command = new DeleteProductCommand { Id = commandId };
        var existingProduct = new Product { Id = commandId, Name = "Laptop" };

        _repositoryMock.Setup(r => r.GetByIdAsync(commandId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.DeleteSoftAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("conflict"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("Concurrent update detected");
    }
}
