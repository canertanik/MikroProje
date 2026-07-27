using FluentAssertions;
using Moq;
using MikroProje.Application.Features.SupplierPayments.Queries.GetSupplierPaymentById;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.SupplierPayments;

public class GetSupplierPaymentByIdQueryHandlerTests : TestBase
{
    private readonly Mock<ISupplierPaymentRepository> _supplierPaymentRepositoryMock;
    private readonly GetSupplierPaymentByIdQueryHandler _handler;

    public GetSupplierPaymentByIdQueryHandlerTests()
    {
        _supplierPaymentRepositoryMock = new Mock<ISupplierPaymentRepository>();
        _handler = new GetSupplierPaymentByIdQueryHandler(_supplierPaymentRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenExists()
    {
        var payment = new SupplierPayment { Id = 1, Amount = 100 };
        _supplierPaymentRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var query = new GetSupplierPaymentByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenNotFound()
    {
        _supplierPaymentRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((SupplierPayment?)null);

        var query = new GetSupplierPaymentByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
    }
}
