using FluentAssertions;
using Moq;
using MikroProje.Application.Features.StockTransfers.Queries.GetStockTransferById;
using MikroProje.Application.Interfaces;
using MikroProje.Application.Features.StockTransfers.DTOs;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.StockTransfers;

public class GetStockTransferByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IStockTransferRepository> _repositoryMock;
    private readonly GetStockTransferByIdQueryHandler _handler;

    public GetStockTransferByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IStockTransferRepository>();
        _handler = new GetStockTransferByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenExists()
    {
        var dto = new StockTransferDto { Id = 1, TransferNumber = "TR-01" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var query = new GetStockTransferByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((StockTransferDto?)null);

        var query = new GetStockTransferByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
    }
}
