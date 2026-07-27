using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Features.AuditLogs.Queries.GetAuditLogById;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.AuditLogs;

public class GetAuditLogByIdQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetAuditLogByIdQueryHandler _handler;

    public GetAuditLogByIdQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAuditLogByIdQueryHandler(_dbContextMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenAuditLogExists()
    {
        var logs = new List<AuditLog> { new AuditLog { Id = 1, EntityName = "User", Action = MikroProje.Domain.Enums.AuditAction.Create } };
        _dbContextMock.Setup(c => c.AuditLogs).ReturnsDbSet(logs);

        var query = new GetAuditLogByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFail_WhenAuditLogDoesNotExist()
    {
        var logs = new List<AuditLog>();
        _dbContextMock.Setup(c => c.AuditLogs).ReturnsDbSet(logs);

        var query = new GetAuditLogByIdQuery { Id = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}

