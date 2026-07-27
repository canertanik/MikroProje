using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Features.StockTransfers.Commands.CancelStockTransfer;
using MikroProje.Application.Interfaces;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.StockTransfers;

public class CancelStockTransferCommandHandlerTests : TestBase
{
    private readonly Mock<IStockTransferRepository> _stockTransferRepositoryMock;
    private readonly CancelStockTransferCommandHandler _handler;

    public CancelStockTransferCommandHandlerTests()
    {
        _stockTransferRepositoryMock = new Mock<IStockTransferRepository>();
        _handler = new CancelStockTransferCommandHandler(_stockTransferRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        _stockTransferRepositoryMock.Setup(r => r.CancelTransferAsync(1, It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new CancelStockTransferCommand { Id = 1, RowVersion = new byte[] { 1, 2, 3 } };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Contain("iptal edildi");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenConflict()
    {
        _stockTransferRepositoryMock.Setup(r => r.CancelTransferAsync(1, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException("Conflict"));

        var command = new CancelStockTransferCommand { Id = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("Conflict");
    }
}
