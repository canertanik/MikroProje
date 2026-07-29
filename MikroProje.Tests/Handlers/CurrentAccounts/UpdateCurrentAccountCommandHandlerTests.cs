using FluentAssertions;
using Moq;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Features.CurrentAccounts.Commands.UpdateCurrentAccount;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.CurrentAccounts;

public class UpdateCurrentAccountCommandHandlerTests : TestBase
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ICurrentAccountRepository> _repositoryMock;
    private readonly UpdateCurrentAccountCommandHandler _handler;

    public UpdateCurrentAccountCommandHandlerTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _repositoryMock = new Mock<ICurrentAccountRepository>();
        _handler = new UpdateCurrentAccountCommandHandler(_repositoryMock.Object, Mapper, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var command = new UpdateCurrentAccountCommand
        {
            Id = 1,
            Code = "120.02",
            Name = "Updated Account"
        };

        var existingAccount = new CurrentAccount { Id = command.Id, Code = "120.01" };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(command.Code);
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenEntityNotFound()
    {
        // Arrange
        var command = new UpdateCurrentAccountCommand { Id = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CurrentAccount?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("not found");
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenCodeAlreadyExists()
    {
        // Arrange
        var command = new UpdateCurrentAccountCommand { Id = 1, Code = "120.02" };
        var existingAccount = new CurrentAccount { Id = command.Id, Code = "120.01" };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("already exists");
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenRepositoryThrowsException()
    {
        // Arrange
        var command = new UpdateCurrentAccountCommand { Id = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database connection failed");
    }
}




