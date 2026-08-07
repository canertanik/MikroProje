using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.SupplierPayments.Queries.GetAllSupplierPayments;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.SupplierPayments;

public class GetAllSupplierPaymentsQueryHandlerTests : TestBase
{
    private readonly Mock<ISupplierPaymentRepository> _supplierPaymentRepositoryMock;
    private readonly GetAllSupplierPaymentsQueryHandler _handler;

    public GetAllSupplierPaymentsQueryHandlerTests()
    {
        _supplierPaymentRepositoryMock = new Mock<ISupplierPaymentRepository>();
        _handler = new GetAllSupplierPaymentsQueryHandler(_supplierPaymentRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var payments = new List<SupplierPayment>
        {
            new SupplierPayment { Id = 1, CurrentAccountId = 1, Amount = 100 },
            new SupplierPayment { Id = 2, CurrentAccountId = 1, Amount = 200 }
        };

        _supplierPaymentRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((payments, 2));

        var query = new GetAllSupplierPaymentsQuery { CurrentAccountId = 1, PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
}
