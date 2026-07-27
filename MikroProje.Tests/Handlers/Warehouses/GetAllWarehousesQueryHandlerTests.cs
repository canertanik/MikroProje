using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.Warehouses.Queries.GetAllWarehouses;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Warehouses;

public class GetAllWarehousesQueryHandlerTests : TestBase
{
    private readonly Mock<IWarehouseRepository> _repositoryMock;
    private readonly GetAllWarehousesQueryHandler _handler;

    public GetAllWarehousesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IWarehouseRepository>();
        _handler = new GetAllWarehousesQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var warehouses = new List<Warehouse>
        {
            new Warehouse { Id = 1, Code = "WH-001", Name = "Ana Depo" },
            new Warehouse { Id = 2, Code = "WH-002", Name = "Şube Depo" }
        };
        var pagedResult = PagedResult<Warehouse>.Create(warehouses, 1, 10, 2);

        _repositoryMock.Setup(r => r.GetAllPagedAsync(It.IsAny<string>(), It.IsAny<bool?>(), It.IsAny<bool?>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var query = new GetAllWarehousesQuery { PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}
