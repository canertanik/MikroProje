using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Sales.Queries.GetSaleById;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Sales;

public class GetSaleByIdQueryHandlerTests : TestBase
{
    private readonly Mock<ISaleRepository> _repositoryMock;
    private readonly GetSaleByIdQueryHandler _handler;

    public GetSaleByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<ISaleRepository>();
        _handler = new GetSaleByIdQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenSaleExists()
    {
        var sale = new Sale { Id = 1 };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(sale);

        var query = new GetSaleByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenSaleNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Sale?)null);

        var query = new GetSaleByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamad");
    }
}

