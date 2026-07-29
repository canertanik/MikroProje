using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Infrastructure.Caching;

namespace MikroProje.Tests.Infrastructure.Caching;

public class RedisCacheServiceTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly Mock<ILogger<RedisCacheService>> _loggerMock;
    private readonly RedisOptions _options;
    private readonly RedisCacheService _cacheService;

    public RedisCacheServiceTests()
    {
        _distributedCacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<RedisCacheService>>();
        _options = new RedisOptions { InstanceName = "Test_", DefaultExpirationMinutes = 10 };

        var optionsMock = new Mock<IOptions<RedisOptions>>();
        optionsMock.Setup(o => o.Value).Returns(_options);

        _cacheService = new RedisCacheService(
            _distributedCacheMock.Object,
            _loggerMock.Object,
            optionsMock.Object,
            null);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDeserializedObject_WhenCacheHit()
    {
        var key = "testkey";
        var expectedValue = new TestData { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(expectedValue);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        _distributedCacheMock.Setup(c => c.GetAsync("testkey", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var result = await _cacheService.GetAsync<TestData>(key);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDefault_WhenCacheMiss()
    {
        var key = "misskey";

        _distributedCacheMock.Setup(c => c.GetAsync("misskey", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        var result = await _cacheService.GetAsync<TestData>(key);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnDefaultAndLogWarning_WhenRedisThrowsException()
    {
        var key = "error_key";

        _distributedCacheMock.Setup(c => c.GetAsync("error_key", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        var result = await _cacheService.GetAsync<TestData>(key);

        result.Should().BeNull();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldReturnFromCache_WhenCacheHit()
    {
        var key = "testkey";
        var cachedValue = new TestData { Id = 2, Name = "Cached" };
        var json = JsonSerializer.Serialize(cachedValue);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        _distributedCacheMock.Setup(c => c.GetAsync("testkey", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        bool factoryCalled = false;

        var result = await _cacheService.GetOrCreateAsync(key, async (ct) =>
        {
            factoryCalled = true;
            return await Task.FromResult(new TestData { Id = 3, Name = "Factory" });
        }, TimeSpan.FromMinutes(5));

        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldCallFactoryAndCacheResult_WhenCacheMiss()
    {
        var key = "misskey";

        _distributedCacheMock.Setup(c => c.GetAsync("misskey", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        var factoryResult = Result<TestData>.Ok(new TestData { Id = 3, Name = "Factory" });

        var result = await _cacheService.GetOrCreateAsync(key, async (ct) =>
        {
            return await Task.FromResult(factoryResult);
        }, TimeSpan.FromMinutes(5));

        result.Should().NotBeNull();
        result!.Data!.Id.Should().Be(3);

        _distributedCacheMock.Verify(c => c.SetAsync(
            "misskey",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_ShouldNotCache_WhenFactoryThrowsException()
    {
        var key = "error_factory";

        _distributedCacheMock.Setup(c => c.GetAsync("error_factory", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        Func<Task> act = async () =>
        {
            await _cacheService.GetOrCreateAsync<TestData>(key, async (ct) =>
            {
                throw new Exception("Factory error");
            }, TimeSpan.FromMinutes(5));
        };

        await act.Should().ThrowAsync<Exception>().WithMessage("Factory error");

        _distributedCacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallDistributedCacheRemove_WithCorrectKey()
    {
        var key = "dashboard:summary";

        await _cacheService.RemoveAsync(key);

        _distributedCacheMock.Verify(c => c.RemoveAsync("dashboard:summary", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ShouldLogWarning_WhenRedisThrowsException()
    {
        var key = "testkey";
        var value = new TestData { Id = 1, Name = "Test" };

        _distributedCacheMock.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis write failed"));

        // Should NOT throw — fail-silent
        await _cacheService.SetAsync(key, value, TimeSpan.FromMinutes(5));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
            Times.Once);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
