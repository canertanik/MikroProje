using FluentAssertions;
using Moq;
using MikroProje.Application.Features.CurrentAccounts.Queries.GetCurrentAccountById;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.CurrentAccounts;

public class GetCurrentAccountByIdQueryHandlerTests : TestBase
{
    private readonly Mock<ICurrentAccountRepository> _repositoryMock;
    private readonly GetCurrentAccountByIdQueryHandler _handler;

    public GetCurrentAccountByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<ICurrentAccountRepository>();
        _handler = new GetCurrentAccountByIdQueryHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAccountExists()
    {
        var account = new CurrentAccount { Id = 1, Name = "Test Account", Code = "CA-001" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(account);

        var query = new GetCurrentAccountByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenAccountNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((CurrentAccount?)null);

        var query = new GetCurrentAccountByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("not found");
    }
}
