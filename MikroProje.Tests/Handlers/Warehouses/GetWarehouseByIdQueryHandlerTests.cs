using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Warehouses.Queries.GetWarehouseById;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Warehouses;

public class GetWarehouseByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly GetWarehouseByIdQueryHandler _handler;

    public GetWarehouseByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _handler = new GetWarehouseByIdQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenWarehouseExists()
    {
        var warehouse = new Warehouse { Id = 1, Code = "WH-001", Name = "Ana Depo" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(warehouse);

        var query = new GetWarehouseByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenWarehouseNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Warehouse?)null);

        var query = new GetWarehouseByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamad");
    }
}
