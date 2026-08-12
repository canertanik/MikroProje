using FluentAssertions;
using Moq;
using MikroProje.Application.Features.SupplierStatements.DTOs;
using MikroProje.Application.Features.SupplierStatements.Queries.GetSupplierStatement;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.SupplierStatements;

public class GetSupplierStatementQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentAccountRepository> _currentAccountRepositoryMock;
    private readonly Mock<IPurchaseRepository> _purchaseRepositoryMock;
    private readonly Mock<ISupplierPaymentRepository> _supplierPaymentRepositoryMock;
    private readonly GetSupplierStatementQueryHandler _handler;

    public GetSupplierStatementQueryHandlerTests()
    {
        _currentAccountRepositoryMock = new Mock<ICurrentAccountRepository>();
        _purchaseRepositoryMock = new Mock<IPurchaseRepository>();
        _supplierPaymentRepositoryMock = new Mock<ISupplierPaymentRepository>();
        
        _handler = new GetSupplierStatementQueryHandler(
            _currentAccountRepositoryMock.Object, 
            _purchaseRepositoryMock.Object, 
            _supplierPaymentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithCorrectRunningBalance()
    {
        var account = new CurrentAccount { Id = 1, Type = CurrentAccountType.Supplier, Name = "Test" };
        _currentAccountRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var purchases = new List<SupplierStatementItemDto>
        {
            new SupplierStatementItemDto { DocumentId = 1, DocumentType = MikroProje.Domain.Enums.DocumentType.Purchase, Date = DateTime.UtcNow.AddDays(-2), Debit = 100, Credit = 0 }
        };

        var payments = new List<SupplierStatementItemDto>
        {
            new SupplierStatementItemDto { DocumentId = 1, DocumentType = MikroProje.Domain.Enums.DocumentType.Payment, Date = DateTime.UtcNow.AddDays(-1), Debit = 0, Credit = 50 }
        };

        _purchaseRepositoryMock.Setup(r => r.GetStatementPurchasesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(purchases);
        _supplierPaymentRepositoryMock.Setup(r => r.GetStatementPaymentsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(payments);

        var query = new GetSupplierStatementQuery { CurrentAccountId = 1, PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.SupplierBalance.Should().Be(50);
        result.Data.Items.Items.Should().HaveCount(2);

        var first = result.Data.Items.Items.First(); // Alış
        first.RunningBalance.Should().Be(100);

        var second = result.Data.Items.Items.Last(); // Ödeme
        second.RunningBalance.Should().Be(50); // 100 (Alış borçlandırır) - 50 (Ödeme alacaklandırır)
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenAccountNotSupplier()
    {
        var account = new CurrentAccount { Id = 1, Type = CurrentAccountType.Customer, Name = "Test" };
        _currentAccountRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var query = new GetSupplierStatementQuery { CurrentAccountId = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
}

