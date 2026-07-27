using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.Products.Queries.GetCriticalStockProducts;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Products;

public class GetCriticalStockProductsQueryHandlerTests : TestBase
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly GetCriticalStockProductsQueryHandler _handler;

    public GetCriticalStockProductsQueryHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _handler = new GetCriticalStockProductsQueryHandler(_productRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", StockQuantity = 5, CriticalStockQuantity = 10 },
            new Product { Id = 2, Name = "Product 2", StockQuantity = 2, CriticalStockQuantity = 5 }
        };

        _productRepositoryMock.Setup(r => r.GetCriticalStockPagedAsync(It.IsAny<string>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));

        var query = new GetCriticalStockProductsQuery { PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}
