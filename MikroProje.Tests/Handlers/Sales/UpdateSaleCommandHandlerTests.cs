using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.Sales.Commands.UpdateSale;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Sales;

public class UpdateSaleCommandHandlerTests : TestBase
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ISaleRepository> _saleRepositoryMock;
    private readonly UpdateSaleCommandHandler _handler;

    public UpdateSaleCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _saleRepositoryMock = new Mock<ISaleRepository>();
        _handler = new UpdateSaleCommandHandler(_saleRepositoryMock.Object, Mapper, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenSaleExists()
    {
        var sale = new Sale { Id = 1, Description = "Old" };
        _saleRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sale);

        var updatedSale = new Sale { Id = 1, Description = "New" };
        _saleRepositoryMock.Setup(r => r.UpdateSaleAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>())).ReturnsAsync(updatedSale);

        var command = new UpdateSaleCommand { Id = 1, Description = "New" };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Description.Should().Be("New");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenSaleNotFound()
    {
        _saleRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Sale?)null);

        var command = new UpdateSaleCommand { Id = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenConcurrencyConflict()
    {
        var sale = new Sale { Id = 1, Description = "Old" };
        _saleRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sale);

        _saleRepositoryMock.Setup(r => r.UpdateSaleAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("Conflict"));

        var command = new UpdateSaleCommand { Id = 1, Description = "New" };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("zamanlı");
    }
}
