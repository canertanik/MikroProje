using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.Sales.Commands.CancelSale;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Sales;

public class CancelSaleCommandHandlerTests : TestBase
{
    private readonly Mock<ISaleRepository> _repositoryMock;
    private readonly CancelSaleCommandHandler _handler;

    public CancelSaleCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISaleRepository>();
        _handler = new CancelSaleCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenSaleExistsAndNotDeleted()
    {
        var command = new CancelSaleCommand { Id = 1 };
        var sale = new Sale { Id = 1, IsDeleted = false };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        _repositoryMock.Setup(r => r.CancelSaleAsync(sale, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(204);
        _repositoryMock.Verify(r => r.CancelSaleAsync(sale, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenSaleNotFound()
    {
        var command = new CancelSaleCommand { Id = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Sale?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamad");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenSaleAlreadyDeleted()
    {
        var command = new CancelSaleCommand { Id = 1 };
        var sale = new Sale { Id = 1, IsDeleted = true };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sale);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("iptal edilmi");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenConcurrencyConflictOccurs()
    {
        var command = new CancelSaleCommand { Id = 1 };
        var sale = new Sale { Id = 1, IsDeleted = false };

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sale);
        _repositoryMock.Setup(r => r.CancelSaleAsync(sale, It.IsAny<CancellationToken>())).ThrowsAsync(new ConcurrencyConflictException("conflict"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("zaman");
    }
}
