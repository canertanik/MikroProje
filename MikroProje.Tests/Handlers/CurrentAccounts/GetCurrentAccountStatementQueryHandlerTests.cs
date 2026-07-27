using FluentAssertions;
using Moq;
using MikroProje.Application.Features.CurrentAccounts.Queries.GetStatement;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.CurrentAccounts;

public class GetCurrentAccountStatementQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentAccountRepository> _repositoryMock;
    private readonly GetCurrentAccountStatementQueryHandler _handler;

    public GetCurrentAccountStatementQueryHandlerTests()
    {
        _repositoryMock = new Mock<ICurrentAccountRepository>();
        _handler = new GetCurrentAccountStatementQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithCalculatedBalances()
    {
        var command = new GetCurrentAccountStatementQuery { CurrentAccountId = 1, PageNumber = 1, PageSize = 10 };

        _repositoryMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var transactions = new List<StatementDto>
        {
            new StatementDto { Date = DateTime.UtcNow.AddDays(-2), DocumentType = MikroProje.Domain.Enums.DocumentType.Sale, DocumentId = 1, Debit = 100, Credit = 0 },
            new StatementDto { Date = DateTime.UtcNow.AddDays(-1), DocumentType = MikroProje.Domain.Enums.DocumentType.Payment, DocumentId = 2, Debit = 0, Credit = 50 }
        };

        _repositoryMock.Setup(r => r.GetStatementTransactionsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(transactions);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        
        // Check running balance logic
        result.Data.Items.First().BalanceAfterTransaction.Should().Be(100);
        result.Data.Items.Last().BalanceAfterTransaction.Should().Be(50); // 100 debit - 50 credit = 50
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenAccountNotFound()
    {
        var command = new GetCurrentAccountStatementQuery { CurrentAccountId = 1 };

        _repositoryMock.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("not found");
    }
}


