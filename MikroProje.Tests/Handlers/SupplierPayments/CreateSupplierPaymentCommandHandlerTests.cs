using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Features.SupplierPayments.Commands.CreateSupplierPayment;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.SupplierPayments;

public class CreateSupplierPaymentCommandHandlerTests : TestBase
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ISupplierPaymentRepository> _supplierPaymentRepositoryMock;
    private readonly Mock<ICurrentAccountRepository> _currentAccountRepositoryMock;
    private readonly CreateSupplierPaymentCommandHandler _handler;

    public CreateSupplierPaymentCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _supplierPaymentRepositoryMock = new Mock<ISupplierPaymentRepository>();
        _currentAccountRepositoryMock = new Mock<ICurrentAccountRepository>();
        _handler = new CreateSupplierPaymentCommandHandler(_supplierPaymentRepositoryMock.Object, _currentAccountRepositoryMock.Object, Mapper, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var account = new CurrentAccount { Id = 1, Type = CurrentAccountType.Supplier, Balance = 500 };
        _currentAccountRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var payment = new SupplierPayment { Id = 1, CurrentAccountId = 1, Amount = 100 };
        _supplierPaymentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<SupplierPayment>(), account, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var command = new CreateSupplierPaymentCommand { CurrentAccountId = 1, Amount = 100 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenAccountNotSupplier()
    {
        var account = new CurrentAccount { Id = 1, Type = CurrentAccountType.Customer, Balance = 500 };
        _currentAccountRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var command = new CreateSupplierPaymentCommand { CurrentAccountId = 1, Amount = 100 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("yalnızca tedarikçi");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenAmountExceedsBalance()
    {
        var account = new CurrentAccount { Id = 1, Type = CurrentAccountType.Supplier, Balance = 50 };
        _currentAccountRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var command = new CreateSupplierPaymentCommand { CurrentAccountId = 1, Amount = 100 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("büyük olamaz");
    }
}
