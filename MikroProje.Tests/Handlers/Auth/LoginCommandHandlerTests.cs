using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using MikroProje.Application.Features.Auth.Commands.Login;
using MikroProje.Application.Features.Auth.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Auth;

public class LoginCommandHandlerTests : TestBase
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(c => c["Jwt:RefreshTokenExpirationDays"]).Returns("7");
        _configurationMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("60");

        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var command = new LoginCommand { Request = new LoginRequestDto { Email = "test@test.com", Password = "Password123" } };
        var user = new AppUser { Id = 1, Email = "test@test.com", PasswordHash = "hash", IsActive = true, Role = MikroProje.Domain.Enums.UserRole.Admin };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword(command.Request.Password, user.PasswordHash)).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("access-token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
        _tokenServiceMock.Setup(t => t.HashRefreshToken("refresh-token")).Returns("hashed-refresh");
        _refreshTokenRepositoryMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().Be("access-token");
        result.Data.RefreshToken.Should().Be("refresh-token");
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenUserNotFound()
    {
        var command = new LoginCommand { Request = new LoginRequestDto { Email = "test@test.com", Password = "Password123" } };
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Request.Email, It.IsAny<CancellationToken>())).ReturnsAsync((AppUser?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Contain("Geçersiz e-posta veya parola");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenPasswordIsInvalid()
    {
        var command = new LoginCommand { Request = new LoginRequestDto { Email = "test@test.com", Password = "WrongPassword" } };
        var user = new AppUser { Id = 1, Email = "test@test.com", PasswordHash = "hash", IsActive = true };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword(command.Request.Password, user.PasswordHash)).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Contain("Geçersiz e-posta veya parola");
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenUserIsPassive()
    {
        var command = new LoginCommand { Request = new LoginRequestDto { Email = "test@test.com", Password = "Password123" } };
        var user = new AppUser { Id = 1, Email = "test@test.com", PasswordHash = "hash", IsActive = false };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.Request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword(command.Request.Password, user.PasswordHash)).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Contain("pasif");
    }
}

