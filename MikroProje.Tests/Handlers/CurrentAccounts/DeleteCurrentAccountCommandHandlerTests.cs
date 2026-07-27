using FluentAssertions;
using Moq;
using MikroProje.Application.Features.CurrentAccounts.Commands.DeleteCurrentAccount;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.CurrentAccounts;

public class DeleteCurrentAccountCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentAccountRepository> _repositoryMock;
    private readonly DeleteCurrentAccountCommandHandler _handler;

    public DeleteCurrentAccountCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICurrentAccountRepository>();
        _handler = new DeleteCurrentAccountCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntityExists()
    {
        // Arrange
        var command = new DeleteCurrentAccountCommand { Id = 1 };
        var existingAccount = new CurrentAccount { Id = command.Id, IsDeleted = false };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(204);
        existingAccount.IsDeleted.Should().BeTrue();
        
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenEntityNotFound()
    {
        // Arrange
        var command = new DeleteCurrentAccountCommand { Id = 1 };

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
    public async Task Handle_ShouldReturnFail_WhenRepositoryThrowsException()
    {
        // Arrange
        var command = new DeleteCurrentAccountCommand { Id = 1 };

        _repositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Connection failed"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Connection failed");
    }
}


