using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.StockMovements.Queries.GetStockMovementsByProduct;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.StockMovements;

public class GetStockMovementsByProductQueryHandlerTests : TestBase
{
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetStockMovementsByProductQueryHandler _handler;

    public GetStockMovementsByProductQueryHandlerTests()
    {
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetStockMovementsByProductQueryHandler(_stockMovementRepositoryMock.Object, _productRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var product = new Product { Id = 1 };
        _productRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var movements = new List<StockMovement>
        {
            new StockMovement { Id = 1, ProductId = 1, Quantity = 10 },
            new StockMovement { Id = 2, ProductId = 1, Quantity = 5 }
        };

        _stockMovementRepositoryMock.Setup(r => r.GetByProductAsync(1, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<StockMovementType?>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((movements, 2));

        var query = new GetStockMovementsByProductQuery { ProductId = 1, PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenProductNotFound()
    {
        _productRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var query = new GetStockMovementsByProductQuery { ProductId = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("not found");
    }
}
