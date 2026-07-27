using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.StockTransfers.Queries.GetAllStockTransfers;
using MikroProje.Application.Interfaces;
using MikroProje.Application.Features.StockTransfers.DTOs;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.StockTransfers;

public class GetAllStockTransfersQueryHandlerTests : TestBase
{
    private readonly Mock<IStockTransferRepository> _repositoryMock;
    private readonly GetAllStockTransfersQueryHandler _handler;

    public GetAllStockTransfersQueryHandlerTests()
    {
        _repositoryMock = new Mock<IStockTransferRepository>();
        _handler = new GetAllStockTransfersQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var items = new List<StockTransferListDto>
        {
            new StockTransferListDto { Id = 1, TransferNumber = "TR-01" },
            new StockTransferListDto { Id = 2, TransferNumber = "TR-02" }
        };
        var pagedResult = PagedResult<StockTransferListDto>.Create(items, 1, 10, 2);

        _repositoryMock.Setup(r => r.GetAllPagedAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<MikroProje.Domain.Enums.StockTransferStatus?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new GetAllStockTransfersQuery { PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}
