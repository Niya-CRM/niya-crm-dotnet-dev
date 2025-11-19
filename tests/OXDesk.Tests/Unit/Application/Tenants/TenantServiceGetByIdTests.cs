using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OXDesk.Application.Tenants;
using OXDesk.Core;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Cache;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
using OXDesk.Core.Tenants;
using OXDesk.Core.DynamicObjects;
using Shouldly;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OXDesk.Tests.Unit.Application.Tenants
{
    public class TenantServiceGetByIdTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<TenantService>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ICurrentTenant> _mockCurrentTenant;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IDynamicObjectService> _mockDynamicObjectService;
        private readonly Mock<ITenantRepository> _mockTenantRepository;
        private readonly Mock<IAuditLogRepository> _mockAuditLogRepository;
        private readonly TenantService _tenantService;

        public TenantServiceGetByIdTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<TenantService>>();
            _mockCacheService = new Mock<ICacheService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockCurrentTenant = new Mock<ICurrentTenant>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockTenantRepository = new Mock<ITenantRepository>();
            _mockAuditLogRepository = new Mock<IAuditLogRepository>();
            _mockDynamicObjectService = new Mock<IDynamicObjectService>();

            // Setup repository factory pattern
            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<ITenantRepository>())
                .Returns(_mockTenantRepository.Object);
            
            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<IAuditLogRepository>())
                .Returns(_mockAuditLogRepository.Object);

            // Setup HttpContextAccessor with a user
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new(ClaimTypes.Name, "testuser"),
                new(ClaimTypes.NameIdentifier, "user123")
            ], "mock"));
        
            var mockHttpContext = new Mock<HttpContext>();
            var mockConnection = new Mock<ConnectionInfo>();
            var mockIpAddress = new Mock<System.Net.IPAddress>(new byte[] { 127, 0, 0, 1 });
        
            mockConnection.Setup(c => c.RemoteIpAddress).Returns(mockIpAddress.Object);
            mockHttpContext.Setup(c => c.Connection).Returns(mockConnection.Object);
            mockHttpContext.Setup(c => c.User).Returns(user);
        
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            _mockDynamicObjectService
                .Setup(s => s.GetDynamicObjectIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _tenantService = new TenantService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockHttpContextAccessor.Object,
                _mockCurrentTenant.Object,
                _mockCacheService.Object,
                _mockCurrentUser.Object,
                _mockDynamicObjectService.Object);
        }

        [Fact]
        public async Task GetTenantByIdAsync_ShouldReturnTenant_WhenTenantExists()
        {
            // Arrange
            var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000065");
            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Host = "test.domain.com",
                Email = "test@example.com",
                IsActive = "Y"
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            // Act
            var result = await _tenantService.GetTenantByIdAsync(tenantId);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(tenantId);
            result.Name.ShouldBe("Test Tenant");
            result.Host.ShouldBe("test.domain.com");
            result.Email.ShouldBe("test@example.com");
            result.IsActive.ShouldBe("Y");

            // Verify tenant was looked up
            _mockTenantRepository.Verify(
                repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTenantByIdAsync_ShouldReturnNull_WhenTenantNotFound()
        {
            // Arrange
            var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000066");

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant?)null);

            // Act
            var result = await _tenantService.GetTenantByIdAsync(tenantId);

            // Assert
            result.ShouldBeNull();

            // Verify tenant was looked up
            _mockTenantRepository.Verify(
                repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
