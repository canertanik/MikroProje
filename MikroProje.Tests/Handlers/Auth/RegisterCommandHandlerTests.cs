using FluentAssertions;
using Moq;
using MikroProje.Application.Features.Auth.Commands.Register;
using MikroProje.Application.Features.Auth.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Auth;

public class RegisterCommandHandlerTests : TestBase
{
    private readonly Mock<IAppUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IAppUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _handler = new RegisterCommandHandler(_userRepositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEmailIsUnique()
    {
        var command = new RegisterCommand 
        { 
            Request = new RegisterRequestDto 
            { 
                Email = "test@test.com", 
                Password = "Password123",
                FirstName = "Caner",
                LastName = "Tanik"
            } 
        };

        _userRepositoryMock.Setup(r => r.IsEmailUniqueAsync(command.Request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _passwordHasherMock.Setup(h => h.HashPassword(command.Request.Password)).Returns("hashed-password");
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be("test@test.com");
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenEmailIsAlreadyInUse()
    {
        var command = new RegisterCommand { Request = new RegisterRequestDto { Email = "test@test.com" } };

        _userRepositoryMock.Setup(r => r.IsEmailUniqueAsync(command.Request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("zaten kullan");
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
