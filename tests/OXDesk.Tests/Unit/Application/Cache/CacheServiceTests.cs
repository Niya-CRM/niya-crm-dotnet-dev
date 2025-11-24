using Microsoft.Extensions.Logging;
using Moq;
using OXDesk.Shared.Services;
using OXDesk.Core.Cache;
using OXDesk.Core.Tenants;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace OXDesk.Tests.Unit.Application.Cache
{
    public class CacheServiceTests
    {
        private readonly Mock<ICacheRepository> _mockCacheRepository;
        private readonly Mock<ILogger<CacheService>> _mockLogger;
        private readonly Mock<ICurrentTenant> _mockCurrentTenant;
        private readonly CacheService _cacheService;
        private readonly string _tenantPrefix;

        public CacheServiceTests()
        {
            _mockCacheRepository = new Mock<ICacheRepository>();
            _mockLogger = new Mock<ILogger<CacheService>>();
            _mockCurrentTenant = new Mock<ICurrentTenant>();

            // Use a deterministic tenant id for assertions
            var tenantId = Guid.Parse("00000000-0000-0000-0000-0000000006A5");
            _mockCurrentTenant.SetupGet(t => t.Id).Returns(tenantId);
            _tenantPrefix = $"t:{tenantId:N}:";

            _cacheService = new CacheService(_mockCacheRepository.Object, _mockLogger.Object, _mockCurrentTenant.Object);
        }

        [Fact]
        public async Task GetAsync_ShouldSanitizeKey_AndCallRepository()
        {
            // Arrange
            var key = "Test Key";
            var sanitizedKey = _tenantPrefix + "testkey";
            var expectedValue = "Test Value";

            _mockCacheRepository
                .Setup(repo => repo.GetAsync<string>(sanitizedKey))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedValue);

            // Verify repository was called with sanitized key
            _mockCacheRepository.Verify(
                repo => repo.GetAsync<string>(sanitizedKey),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnNull_WhenKeyNotFound()
        {
            // Arrange
            var key = "NonExistent Key";
            var sanitizedKey = _tenantPrefix + "nonexistentkey";

            _mockCacheRepository
                .Setup(repo => repo.GetAsync<string>(sanitizedKey))
                .ReturnsAsync(default(string));

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.ShouldBeNull();

            // Verify repository was called with sanitized key
            _mockCacheRepository.Verify(
                repo => repo.GetAsync<string>(sanitizedKey),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_ShouldSanitizeKey_AndCallRepository()
        {
            // Arrange
            var key = "Test Key";
            var sanitizedKey = _tenantPrefix + "testkey";
            var value = "Test Value";
            var absoluteExpiration = TimeSpan.FromMinutes(60);
            var slidingExpiration = TimeSpan.FromMinutes(15);

            _mockCacheRepository
                .Setup(repo => repo.SetAsync(sanitizedKey, value, absoluteExpiration, slidingExpiration))
                .Returns(Task.CompletedTask);

            // Act
            await _cacheService.SetAsync(key, value, absoluteExpiration, slidingExpiration);

            // Assert
            // Verify repository was called with sanitized key and provided values
            _mockCacheRepository.Verify(
                repo => repo.SetAsync(sanitizedKey, value, absoluteExpiration, slidingExpiration),
                Times.Once);
        }

        [Fact]
        public async Task SetAsync_ShouldUseDefaultExpirations_WhenNotProvided()
        {
            // Arrange
            var key = "Test Key";
            var sanitizedKey = _tenantPrefix + "testkey";
            var value = "Test Value";

            _mockCacheRepository
                .Setup(repo => repo.SetAsync(
                    sanitizedKey, 
                    value, 
                    It.Is<TimeSpan?>(ts => ts == CacheConstant.DEFAULT_CACHE_EXPIRATION), 
                    It.Is<TimeSpan?>(ts => ts == CacheConstant.DEFAULT_CACHE_SLIDING_EXPIRATION)))
                .Returns(Task.CompletedTask);

            // Act
            await _cacheService.SetAsync(key, value);

            // Assert
            // Verify repository was called with sanitized key and default expirations
            _mockCacheRepository.Verify(
                repo => repo.SetAsync(
                    sanitizedKey, 
                    value, 
                    It.Is<TimeSpan?>(ts => ts == CacheConstant.DEFAULT_CACHE_EXPIRATION), 
                    It.Is<TimeSpan?>(ts => ts == CacheConstant.DEFAULT_CACHE_SLIDING_EXPIRATION)),
                Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_ShouldSanitizeKey_AndCallRepository()
        {
            // Arrange
            var key = "Test Key";
            var sanitizedKey = _tenantPrefix + "testkey";

            _mockCacheRepository
                .Setup(repo => repo.RemoveAsync(sanitizedKey))
                .Returns(Task.CompletedTask);

            // Act
            await _cacheService.RemoveAsync(key);

            // Assert
            // Verify repository was called with sanitized key
            _mockCacheRepository.Verify(
                repo => repo.RemoveAsync(sanitizedKey),
                Times.Once);
        }

        [Fact]
        public void SanitizeKey_ShouldThrowArgumentException_WhenKeyIsNullOrWhitespace()
        {
            // Act & Assert
            var exception1 = Should.Throw<ArgumentException>(() => _cacheService.GetAsync<string>(null!).GetAwaiter().GetResult());
            exception1.Message.ShouldContain("Cache key cannot be null or whitespace");
            
            var exception2 = Should.Throw<ArgumentException>(() => _cacheService.GetAsync<string>("").GetAwaiter().GetResult());
            exception2.Message.ShouldContain("Cache key cannot be null or whitespace");
            
            var exception3 = Should.Throw<ArgumentException>(() => _cacheService.GetAsync<string>("   ").GetAwaiter().GetResult());
            exception3.Message.ShouldContain("Cache key cannot be null or whitespace");
        }

        [Fact]
        public async Task SanitizeKey_ShouldTrimAndLowercaseKey()
        {
            // Arrange
            var key = "  Test Key  ";
            var sanitizedKey = _tenantPrefix + "testkey";
            var expectedValue = "Test Value";

            _mockCacheRepository
                .Setup(repo => repo.GetAsync<string>(sanitizedKey))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.ShouldBe(expectedValue);

            // Verify repository was called with sanitized key
            _mockCacheRepository.Verify(
                repo => repo.GetAsync<string>(sanitizedKey),
                Times.Once);
        }

        [Fact]
        public async Task SanitizeKey_ShouldRemoveInvalidCharacters()
        {
            // Arrange
            var key = "Test@Key#123!";
            var sanitizedKey = _tenantPrefix + "testkey123";
            var expectedValue = "Test Value";

            _mockCacheRepository
                .Setup(repo => repo.GetAsync<string>(sanitizedKey))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.ShouldBe(expectedValue);

            // Verify repository was called with sanitized key
            _mockCacheRepository.Verify(
                repo => repo.GetAsync<string>(sanitizedKey),
                Times.Once);
        }

        [Fact]
        public async Task SanitizeKey_ShouldPreserveColonsAndUnderscores()
        {
            // Arrange
            var key = "test:key_value";
            var sanitizedKey = _tenantPrefix + "test:key_value";
            var expectedValue = "Test Value";

            _mockCacheRepository
                .Setup(repo => repo.GetAsync<string>(sanitizedKey))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            result.ShouldBe(expectedValue);

            // Verify repository was called with sanitized key
            _mockCacheRepository.Verify(
                repo => repo.GetAsync<string>(sanitizedKey),
                Times.Once);
        }
    }
}
