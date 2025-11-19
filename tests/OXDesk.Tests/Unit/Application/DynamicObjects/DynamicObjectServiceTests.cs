using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OXDesk.Application.DynamicObjects;
using OXDesk.Core;
using OXDesk.Core.Cache;
using OXDesk.Core.DynamicObjects;
using OXDesk.Core.Identity;
using OXDesk.Tests.Helpers;
using Shouldly;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace OXDesk.Tests.Unit.Application.DynamicObjects
{
    public class DynamicObjectServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<DynamicObjectService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IDynamicObjectRepository> _mockDynamicObjectRepository;
        private readonly DynamicObjectService _service;

        public DynamicObjectServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<DynamicObjectService>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockCacheService = new Mock<ICacheService>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockDynamicObjectRepository = new Mock<IDynamicObjectRepository>();

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<IDynamicObjectRepository>())
                .Returns(_mockDynamicObjectRepository.Object);

            // Current user setup (required by service)
            _mockCurrentUser.SetupGet(u => u.Id).Returns(TestHelpers.TestUserId1);

            _service = new DynamicObjectService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockHttpContextAccessor.Object,
                _mockCacheService.Object,
                _mockCurrentUser.Object);
        }

        [Fact]
        public async Task GetDynamicObjectIdAsync_ShouldThrowValidationException_WhenKeyIsNullOrWhitespace()
        {
            await Should.ThrowAsync<ValidationException>(() => _service.GetDynamicObjectIdAsync(null!));
            await Should.ThrowAsync<ValidationException>(() => _service.GetDynamicObjectIdAsync(string.Empty));
            await Should.ThrowAsync<ValidationException>(() => _service.GetDynamicObjectIdAsync("   "));
        }

        [Fact]
        public async Task GetDynamicObjectIdAsync_ShouldReturnIdFromCache_WhenPresent()
        {
            var objectKey = "users";
            var normalizedKey = objectKey.Trim();
            var cacheKey = $"dynamic_object:key:{normalizedKey}";
            var cachedId = 123;

            _mockCacheService
                .Setup(c => c.GetAsync<int?>(cacheKey))
                .ReturnsAsync(cachedId);

            var result = await _service.GetDynamicObjectIdAsync(objectKey);

            result.ShouldBe(cachedId);

            _mockCacheService.Verify(c => c.GetAsync<int?>(cacheKey), Times.Once);
            _mockDynamicObjectRepository.Verify(r => r.GetByKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetDynamicObjectIdAsync_ShouldQueryRepositoryAndCache_WhenNotInCache()
        {
            var objectKey = "users";
            var normalizedKey = objectKey.Trim();
            var cacheKey = $"dynamic_object:key:{normalizedKey}";
            var dynamicObject = new DynamicObject { Id = 456, ObjectKey = normalizedKey };

            _mockCacheService
                .Setup(c => c.GetAsync<int?>(cacheKey))
                .ReturnsAsync((int?)null);

            _mockDynamicObjectRepository
                .Setup(r => r.GetByKeyAsync(normalizedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dynamicObject);

            var result = await _service.GetDynamicObjectIdAsync(objectKey);

            result.ShouldBe(dynamicObject.Id);

            _mockCacheService.Verify(c => c.GetAsync<int?>(cacheKey), Times.Once);
            _mockDynamicObjectRepository.Verify(r => r.GetByKeyAsync(normalizedKey, It.IsAny<CancellationToken>()), Times.Once);
            _mockCacheService.Verify(c => c.SetAsync(cacheKey, dynamicObject.Id, null, null), Times.Once);
        }

        [Fact]
        public async Task GetDynamicObjectIdAsync_ShouldThrowInvalidOperationException_WhenObjectNotFound()
        {
            var objectKey = "unknown";
            var normalizedKey = objectKey.Trim();
            var cacheKey = $"dynamic_object:key:{normalizedKey}";

            _mockCacheService
                .Setup(c => c.GetAsync<int?>(cacheKey))
                .ReturnsAsync((int?)null);

            _mockDynamicObjectRepository
                .Setup(r => r.GetByKeyAsync(normalizedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DynamicObject)null!);

            await Should.ThrowAsync<InvalidOperationException>(() => _service.GetDynamicObjectIdAsync(objectKey));

            _mockCacheService.Verify(c => c.GetAsync<int?>(cacheKey), Times.Once);
            _mockDynamicObjectRepository.Verify(r => r.GetByKeyAsync(normalizedKey, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
