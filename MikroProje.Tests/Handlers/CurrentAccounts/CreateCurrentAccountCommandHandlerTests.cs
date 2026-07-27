using FluentAssertions;
using Moq;
using MikroProje.Application.Features.CurrentAccounts.Commands.CreateCurrentAccount;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.CurrentAccounts;

public class CreateCurrentAccountCommandHandlerTests : TestBase
{
    private readonly Mock<ICurrentAccountRepository> _repositoryMock;
    private readonly CreateCurrentAccountCommandHandler _handler;

    public CreateCurrentAccountCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICurrentAccountRepository>();
        _handler = new CreateCurrentAccountCommandHandler(_repositoryMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateCurrentAccountCommand
        {
            Code = "120.01",
            Name = "Test Account",
            TaxNumber = "1234567890",
            Phone = "1234567",
            Email = "test@test.com",
            Type = MikroProje.Domain.Enums.CurrentAccountType.Customer
        };

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<CurrentAccount>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _repositoryMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Should().NotBeNull();
        result.Data!.Code.Should().Be(command.Code);
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<CurrentAccount>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenCodeAlreadyExists()
    {
        // Arrange
        var command = new CreateCurrentAccountCommand { Code = "120.01" };

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("already exists");
        result.Data.Should().BeNull();
        
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<CurrentAccount>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenRepositoryThrowsException()
    {
        // Arrange
        var command = new CreateCurrentAccountCommand { Code = "120.01" };

        _repositoryMock.Setup(r => r.CodeExistsAsync(command.Code, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }
}



