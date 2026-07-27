using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Payments.Commands.DeletePayment;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Payments;

public class DeletePaymentCommandHandlerTests : TestBase
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly DeletePaymentCommandHandler _handler;

    public DeletePaymentCommandHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _handler = new DeletePaymentCommandHandler(_paymentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPaymentExists()
    {
        var payment = new Payment { Id = 1 };
        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var command = new DeletePaymentCommand { Id = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(204);
        
        _paymentRepositoryMock.Verify(r => r.DeleteAsync(payment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenPaymentNotFound()
    {
        _paymentRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Payment?)null);

        var command = new DeletePaymentCommand { Id = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
        
        _paymentRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
