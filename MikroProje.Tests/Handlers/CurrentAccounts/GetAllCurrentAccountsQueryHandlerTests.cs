using FluentAssertions;
using Moq;
using MikroProje.Application.Features.CurrentAccounts.Queries.GetAllCurrentAccounts;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.CurrentAccounts;

public class GetAllCurrentAccountsQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentAccountRepository> _repositoryMock;
    private readonly GetAllCurrentAccountsQueryHandler _handler;

    public GetAllCurrentAccountsQueryHandlerTests()
    {
        _repositoryMock = new Mock<ICurrentAccountRepository>();
        _handler = new GetAllCurrentAccountsQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithAllAccounts()
    {
        var accounts = new List<CurrentAccount>
        {
            new CurrentAccount { Id = 1, Name = "Acc1" },
            new CurrentAccount { Id = 2, Name = "Acc2" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(accounts);

        var query = new GetAllCurrentAccountsQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(2);
    }
}
