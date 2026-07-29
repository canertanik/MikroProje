using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using MikroProje.Application.Features.Auth.Commands.RefreshToken;
using MikroProje.Application.Features.Auth.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Auth;

public class RefreshTokenCommandHandlerTests : TestBase
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _userRepositoryMock = new Mock<IAppUserRepository>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["Jwt:RefreshTokenExpirationDays"]).Returns("7");
        _configurationMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("60");

        _handler = new RefreshTokenCommandHandler(
            _tokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _userRepositoryMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTokensAreValid()
    {
        var command = new RefreshTokenCommand { Request = new RefreshTokenRequestDto { AccessToken = "access", RefreshToken = "refresh" } };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        var existingToken = new RefreshToken { UserId = 1, TokenHash = "hash", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        var user = new AppUser { Id = 1, IsActive = true, Role = MikroProje.Domain.Enums.UserRole.Admin, Email = "test@test.com" };

        _tokenServiceMock.Setup(t => t.GetPrincipalFromExpiredToken("access")).Returns(principal);
        _tokenServiceMock.Setup(t => t.HashRefreshToken("refresh")).Returns("hash");
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh");
        _tokenServiceMock.Setup(t => t.HashRefreshToken("new-refresh")).Returns("new-hash");
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("new-access");

        _refreshTokenRepositoryMock.Setup(r => r.UpdateAsync(existingToken, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _refreshTokenRepositoryMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("new-access");
        result.Data!.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenPrincipalIsInvalid()
    {
        var command = new RefreshTokenCommand { Request = new RefreshTokenRequestDto { AccessToken = "access", RefreshToken = "refresh" } };
        _tokenServiceMock.Setup(t => t.GetPrincipalFromExpiredToken("access")).Returns((ClaimsPrincipal?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Contain("Geçersiz access token");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenRefreshTokenNotFound()
    {
        var command = new RefreshTokenCommand { Request = new RefreshTokenRequestDto { AccessToken = "access", RefreshToken = "refresh" } };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        
        _tokenServiceMock.Setup(t => t.GetPrincipalFromExpiredToken("access")).Returns(principal);
        _tokenServiceMock.Setup(t => t.HashRefreshToken("refresh")).Returns("hash");
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Contain("Geçersiz refresh token");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenRefreshTokenIsRevoked()
    {
        var command = new RefreshTokenCommand { Request = new RefreshTokenRequestDto { AccessToken = "access", RefreshToken = "refresh" } };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }));
        var existingToken = new RefreshToken { UserId = 1, TokenHash = "hash", RevokedAt = DateTime.UtcNow }; // IsRevoked is true
        
        _tokenServiceMock.Setup(t => t.GetPrincipalFromExpiredToken("access")).Returns(principal);
        _tokenServiceMock.Setup(t => t.HashRefreshToken("refresh")).Returns("hash");
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenHashAsync("hash", It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Contain("iptal edilmiş");
    }
}

