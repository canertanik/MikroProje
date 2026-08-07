using FluentAssertions;
using Moq;
using Microsoft.Extensions.Options;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Dashboard.Queries;
using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Tests.Common;

namespace MikroProje.Tests.Handlers.Dashboard;

public class GetDashboardSummaryQueryHandlerTests : TestBase
{
    private readonly Mock<IDashboardRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetDashboardSummaryQueryHandler _handler;

    public GetDashboardSummaryQueryHandlerTests()
    {
        _repositoryMock = new Mock<IDashboardRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        
        var options = Options.Create(new RedisOptions { DashboardExpirationMinutes = 5 });
        
        _handler = new GetDashboardSummaryQueryHandler(_repositoryMock.Object, _cacheServiceMock.Object, options);
    }

    [Fact]
    public async Task Handle_ShouldReturnFromCache_WhenCacheHit()
    {
        var summary = new DashboardSummaryDto { ActiveProductCount = 500 };
        var cachedResult = Result<DashboardSummaryDto>.Ok(summary);

        // Mock GetOrCreateAsync to simulate cache HIT (factory is NOT called)
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            CacheKeys.DashboardSummary(null, null),
            It.IsAny<Func<CancellationToken, Task<Result<DashboardSummaryDto>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);

        var query = new GetDashboardSummaryQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.ActiveProductCount.Should().Be(500);

        // Verify repository was NOT called
        _repositoryMock.Verify(r => r.GetSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallRepository_WhenCacheMiss()
    {
        var dbSummary = new DashboardSummaryDto { ActiveProductCount = 300 };

        _repositoryMock.Setup(r => r.GetSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbSummary);

        // Mock GetOrCreateAsync to invoke the factory
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            CacheKeys.DashboardSummary(null, null),
            It.IsAny<Func<CancellationToken, Task<Result<DashboardSummaryDto>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<CancellationToken, Task<Result<DashboardSummaryDto>>> factory, TimeSpan exp, CancellationToken ct) => factory(ct));

        var query = new GetDashboardSummaryQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.ActiveProductCount.Should().Be(300);

        // Verify repository WAS called
        _repositoryMock.Verify(r => r.GetSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFallbackToRepository_WhenRedisThrowsException()
    {
        var dbSummary = new DashboardSummaryDto { ActiveProductCount = 200 };

        _repositoryMock.Setup(r => r.GetSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbSummary);

        // Redis exception — GetOrCreateAsync should invoke the factory as fallback
        _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
            CacheKeys.DashboardSummary(null, null),
            It.IsAny<Func<CancellationToken, Task<Result<DashboardSummaryDto>>>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()))
            .Returns((string key, Func<CancellationToken, Task<Result<DashboardSummaryDto>>> factory, TimeSpan exp, CancellationToken ct) => factory(ct));

        var query = new GetDashboardSummaryQuery();
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Data!.ActiveProductCount.Should().Be(200);

        // Repository MUST be called when Redis fails
        _repositoryMock.Verify(r => r.GetSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
