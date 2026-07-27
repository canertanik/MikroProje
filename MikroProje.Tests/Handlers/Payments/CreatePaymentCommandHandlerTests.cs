using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Payments.Commands.CreatePayment;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;
using MikroProje.Domain.Enums;

namespace MikroProje.Tests.Handlers.Payments;

public class CreatePaymentCommandHandlerTests : TestBase
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ICurrentAccountRepository> _currentAccountRepositoryMock;
    private readonly CreatePaymentCommandHandler _handler;

    public CreatePaymentCommandHandlerTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _currentAccountRepositoryMock = new Mock<ICurrentAccountRepository>();
        _handler = new CreatePaymentCommandHandler(_paymentRepositoryMock.Object, _currentAccountRepositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAccountExists()
    {
        var account = new CurrentAccount { Id = 1, Name = "Test" };
        _currentAccountRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var payment = new Payment { Id = 1, CurrentAccountId = 1, Amount = 100, Type = PaymentType.Collection };
        _paymentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Payment>(), account, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var command = new CreatePaymentCommand 
        { 
            CurrentAccountId = 1, 
            Amount = 100, 
            Type = PaymentType.Collection, 
            PaymentMethod = PaymentMethod.Cash, 
            PaymentDate = DateTime.UtcNow 
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenAccountNotFound()
    {
        _currentAccountRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((CurrentAccount?)null);

        var command = new CreatePaymentCommand { CurrentAccountId = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
    }
}
