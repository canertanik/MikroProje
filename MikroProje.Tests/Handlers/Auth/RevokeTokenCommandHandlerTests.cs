using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Auth.Commands.RevokeToken;
using MikroProje.Application.Features.Auth.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Auth;

public class RevokeTokenCommandHandlerTests : TestBase
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandHandlerTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _handler = new RevokeTokenCommandHandler(_tokenServiceMock.Object, _refreshTokenRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTokenIsFoundAndActive()
    {
        var command = new RevokeTokenCommand { Request = new RevokeTokenRequestDto { RefreshToken = "refresh" } };
        var existingToken = new RefreshToken { UserId = 1, TokenHash = "hash" };

        _tokenServiceMock.Setup(t => t.HashRefreshToken("refresh")).Returns("hash");
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);
        _refreshTokenRepositoryMock.Setup(r => r.UpdateAsync(existingToken, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        existingToken.IsRevoked.Should().BeTrue();
        existingToken.RevokedAt.Should().NotBeNull();
        _refreshTokenRepositoryMock.Verify(r => r.UpdateAsync(existingToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenTokenNotFound()
    {
        var command = new RevokeTokenCommand { Request = new RevokeTokenRequestDto { RefreshToken = "refresh" } };
        _tokenServiceMock.Setup(t => t.HashRefreshToken("refresh")).Returns("hash");
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamad");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenTokenIsAlreadyRevoked()
    {
        var command = new RevokeTokenCommand { Request = new RevokeTokenRequestDto { RefreshToken = "refresh" } };
        var existingToken = new RefreshToken { UserId = 1, TokenHash = "hash", RevokedAt = DateTime.UtcNow };

        _tokenServiceMock.Setup(t => t.HashRefreshToken("refresh")).Returns("hash");
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("zaten iptal edilmiş");
    }
}
