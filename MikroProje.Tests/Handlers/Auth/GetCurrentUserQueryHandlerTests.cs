using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Auth.Queries.GetCurrentUser;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Auth;

public class GetCurrentUserQueryHandlerTests : TestBase
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly GetCurrentUserQueryHandler _handler;

    public GetCurrentUserQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();
        _handler = new GetCurrentUserQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserExistsAndActive()
    {
        var user = new AppUser { Id = 1, IsActive = true, FirstName = "Caner", LastName = "Tanik", Email = "test@test.com", Role = MikroProje.Domain.Enums.UserRole.Admin };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var query = new GetCurrentUserQuery { UserId = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.FirstName.Should().Be("Caner");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenUserNotFound()
    {
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((AppUser?)null);

        var query = new GetCurrentUserQuery { UserId = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Kullanıcı bulunamadı");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenUserIsPassive()
    {
        var user = new AppUser { Id = 1, IsActive = false };
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var query = new GetCurrentUserQuery { UserId = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("pasif");
    }
}

