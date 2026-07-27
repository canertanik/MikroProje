using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Products.Queries.GetAllProducts;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Products;

public class GetAllProductsQueryHandlerTests : TestBase
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly GetAllProductsQueryHandler _handler;

    public GetAllProductsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _handler = new GetAllProductsQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Prod1" },
            new Product { Id = 2, Name = "Prod2" }
        };
        var totalCount = 2;

        _repositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<string>(), It.IsAny<bool?>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, totalCount));

        var query = new GetAllProductsQuery { PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}
