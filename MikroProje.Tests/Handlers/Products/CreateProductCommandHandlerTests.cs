using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.Products.Commands.CreateProduct;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Products;

public class CreateProductCommandHandlerTests : TestBase
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new CreateProductCommandHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Code = "PRD-001",
            Name = "Test Product",
            Barcode = "123456789",
            PurchasePrice = 10,
            SalePrice = 20,
            VatRate = 18,
            CriticalStockQuantity = 5,
            InitialStockQuantity = 100
        };

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _repositoryMock.Setup(r => r.BarcodeExistsAsync(command.Barcode, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _repositoryMock.Setup(r => r.CreateWithInitialStockAsync(It.IsAny<Product>(), command.InitialStockQuantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, int initialStock, CancellationToken ct) => p);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(command.Code);
        
        _repositoryMock.Verify(r => r.CreateWithInitialStockAsync(It.IsAny<Product>(), command.InitialStockQuantity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenCodeAlreadyExists()
    {
        // Arrange
        var command = new CreateProductCommand { Code = "PRD-001", Barcode = "123456789" };

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("code already exists");
        
        _repositoryMock.Verify(r => r.CreateWithInitialStockAsync(It.IsAny<Product>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenBarcodeAlreadyExists()
    {
        // Arrange
        var command = new CreateProductCommand { Code = "PRD-001", Barcode = "123456789" };

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock.Setup(r => r.BarcodeExistsAsync(command.Barcode, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("barcode already exists");
        
        _repositoryMock.Verify(r => r.CreateWithInitialStockAsync(It.IsAny<Product>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenConcurrencyConflictOccurs()
    {
        // Arrange
        var command = new CreateProductCommand { Code = "PRD-001", Barcode = "123456789" };

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _repositoryMock.Setup(r => r.BarcodeExistsAsync(command.Barcode, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _repositoryMock.Setup(r => r.CreateWithInitialStockAsync(It.IsAny<Product>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("conflict"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("Concurrent update detected");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenRepositoryThrowsException()
    {
        // Arrange
        var command = new CreateProductCommand { Code = "PRD-001" };

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }
}


