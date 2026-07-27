using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Payments.Commands.UpdatePayment;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Payments;

public class UpdatePaymentCommandHandlerTests : TestBase
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly UpdatePaymentCommandHandler _handler;

    public UpdatePaymentCommandHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _handler = new UpdatePaymentCommandHandler(_paymentRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPaymentExists()
    {
        var payment = new Payment { Id = 1, Description = "Old", RowVersion = new byte[] { 1, 2, 3 } };
        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var updatedPayment = new Payment { Id = 1, Description = "New", RowVersion = new byte[] { 1, 2, 4 } };
        _paymentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(updatedPayment);

        var command = new UpdatePaymentCommand { Id = 1, Description = "New", RowVersion = new byte[] { 1, 2, 3 } };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Description.Should().Be("New");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenPaymentNotFound()
    {
        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);

        var command = new UpdatePaymentCommand { Id = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
    }
}
