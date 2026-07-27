using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using MikroProje.Application.Features.AuditLogs.Queries.GetAllAuditLogs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.AuditLogs;

public class GetAllAuditLogsQueryHandlerTests : TestBase
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly GetAllAuditLogsQueryHandler _handler;

    public GetAllAuditLogsQueryHandlerTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAllAuditLogsQueryHandler(_dbContextMock.Object, Mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithPagedData()
    {
        var logs = new List<AuditLog> 
        { 
            new AuditLog { Id = 1, EntityName = "User", Action = MikroProje.Domain.Enums.AuditAction.Create, CreatedDate = DateTime.UtcNow },
            new AuditLog { Id = 2, EntityName = "Product", Action = MikroProje.Domain.Enums.AuditAction.Update, CreatedDate = DateTime.UtcNow }
        };
        _dbContextMock.Setup(c => c.AuditLogs).ReturnsDbSet(logs);

        var query = new GetAllAuditLogsQuery { PageNumber = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
    
    [Fact]
    public async Task Handle_ShouldFilterByEntityName()
    {
        var logs = new List<AuditLog> 
        { 
            new AuditLog { Id = 1, EntityName = "User", Action = MikroProje.Domain.Enums.AuditAction.Create },
            new AuditLog { Id = 2, EntityName = "Product", Action = MikroProje.Domain.Enums.AuditAction.Update }
        };
        _dbContextMock.Setup(c => c.AuditLogs).ReturnsDbSet(logs);

        var query = new GetAllAuditLogsQuery { EntityName = "User" };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().EntityName.Should().Be("User");
    }
}

