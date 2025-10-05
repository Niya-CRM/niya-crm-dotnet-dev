using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OXDesk.Application.Tenants;
using OXDesk.Core;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.Cache;
using OXDesk.Core.Common;
using OXDesk.Core.Tenants;
using OXDesk.Core.Tenants.DTOs;
using OXDesk.Tests.Helpers;
using Shouldly;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OXDesk.Tests.Unit.Application.Tenants
{
    public class TenantServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<TenantService>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ICurrentTenant> _mockCurrentTenant;
        private readonly Mock<ITenantRepository> _mockTenantRepository;
        private readonly Mock<IAuditLogRepository> _mockAuditLogRepository;
        private readonly TenantService _tenantService;

        public TenantServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<TenantService>>();
            _mockCacheService = new Mock<ICacheService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockCurrentTenant = new Mock<ICurrentTenant>();
            _mockTenantRepository = new Mock<ITenantRepository>();
            _mockAuditLogRepository = new Mock<IAuditLogRepository>();

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
            _tenantService = new TenantService(
                _mockUnitOfWork.Object,
                _mockLogger.Object,
                _mockHttpContextAccessor.Object,
                _mockCurrentTenant.Object,
                _mockCacheService.Object);
        }

        [Fact]
        public async Task CreateTenantAsync_ShouldThrowValidationException_WhenNameIsEmpty()
        {
            // Arrange
            var createRequest = new CreateTenantRequest
            {
                Name = "",
                Host = "test.domain.com",
                Email = "test@example.com",
                UserId = TestHelpers.TestUserId1,
                TimeZone = "UTC"
            };

            // Act & Assert
            var exception = await Should.ThrowAsync<ValidationException>(
                async () => await _tenantService.CreateTenantAsync(createRequest));

            exception.Message.ShouldContain("Name");
        }

        [Fact]
        public async Task CreateTenantAsync_ShouldThrowValidationException_WhenHostIsEmpty()
        {
            // Arrange
            var createRequest = new CreateTenantRequest
            {
                Name = "Test Tenant",
                Host = "",
                Email = "test@example.com",
                UserId = TestHelpers.TestUserId1,
                TimeZone = "UTC"
            };

            // Act & Assert
            var exception = await Should.ThrowAsync<ValidationException>(
                async () => await _tenantService.CreateTenantAsync(createRequest));

            exception.Message.ShouldContain("Host");
        }

        [Fact]
        public async Task CreateTenantAsync_ShouldThrowValidationException_WhenEmailIsEmpty()
        {
            // Arrange
            var createRequest = new CreateTenantRequest
            {
                Name = "Test Tenant",
                Host = "test.domain.com",
                Email = "",
                UserId = TestHelpers.TestUserId1,
                TimeZone = "UTC"
            };

            // Act & Assert
            var exception = await Should.ThrowAsync<ValidationException>(
                async () => await _tenantService.CreateTenantAsync(createRequest));

            exception.Message.ShouldContain("Email");
        }

        [Fact]
        public async Task CreateTenantAsync_ShouldThrowInvalidOperationException_WhenHostAlreadyExists()
        {
            // Arrange
            var tenantHost = "existing.domain.com";
            var createRequest = new CreateTenantRequest
            {
                Name = "Test Tenant",
                Host = tenantHost,
                Email = "test@example.com",
                UserId = TestHelpers.TestUserId1,
                TimeZone = "UTC"
            };
            
            var existingTenant = new Tenant
            {
                Id = Guid.Parse("00000000-0000-0000-0000-0000000000C8"),
                Name = "Existing Tenant",
                Host = tenantHost,
                Email = "existing@example.com",
                IsActive = "Y"
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByHostAsync(tenantHost.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTenant);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _tenantService.CreateTenantAsync(createRequest));

            exception.Message.ShouldContain(tenantHost.Trim().ToLowerInvariant());
            
            // Verify host was checked
            _mockTenantRepository.Verify(
                repo => repo.GetByHostAsync(tenantHost.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verify tenant was not added
            _mockTenantRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task AnyTenantsExistAsync_ShouldReturnTrue_WhenTenantsExist()
        {
            // Arrange
            var tenants = new List<Tenant>
            {
                new() { Id = Guid.Parse("00000000-0000-0000-0000-0000000000C9"), Name = "Tenant 1" }
            };

            _mockTenantRepository
                .Setup(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenants);

            // Act
            var result = await _tenantService.AnyTenantsExistAsync(CancellationToken.None);

            // Assert
            result.ShouldBeTrue();
            
            // Verify repository was called
            _mockTenantRepository.Verify(
                repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AnyTenantsExistAsync_ShouldReturnFalse_WhenNoTenantsExist()
        {
            // Arrange
            var tenants = new List<Tenant>();

            _mockTenantRepository
                .Setup(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenants);

            // Act
            var result = await _tenantService.AnyTenantsExistAsync(CancellationToken.None);

            // Assert
            result.ShouldBeFalse();
            
            // Verify repository was called
            _mockTenantRepository.Verify(
                repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task ChangeTenantActivationStatus_Activate_ShouldActivateTenant_AndLogAudit()
        {
            // Arrange
            var tenantId = Guid.Parse("00000000-0000-0000-0000-0000000000D2");
            var reason = "Test activation";
            var tenant = new Tenant
            {
                Id = tenantId,
                Name = "Test Tenant",
                Host = "test.domain.com",
                Email = "test@example.com",
                IsActive = "N"
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            _mockTenantRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant t, CancellationToken ct) => t);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _tenantService.ChangeTenantActivationStatusAsync(tenantId, TenantConstant.ActivationAction.Activate, reason, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.IsActive.ShouldBe("Y");

            // Verify tenant was retrieved
            _mockTenantRepository.Verify(
                repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify tenant was updated
            _mockTenantRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify audit log was created
            _mockAuditLogRepository.Verify(
                repo => repo.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify cache was invalidated
            _mockCacheService.Verify(
                cache => cache.RemoveAsync(It.IsAny<string>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task ChangeTenantActivationStatus_Deactivate_ShouldDeactivateTenant_AndLogAudit()
        {
            // Arrange
            var tenantId = Guid.Parse("00000000-0000-0000-0000-0000000000D3");
            var reason = "Test deactivation";
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

            _mockTenantRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant t, CancellationToken ct) => t);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _tenantService.ChangeTenantActivationStatusAsync(tenantId, TenantConstant.ActivationAction.Deactivate, reason);

            // Assert
            result.ShouldNotBeNull();
            result.IsActive.ShouldBe("N");

            // Verify tenant was retrieved
            _mockTenantRepository.Verify(
                repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify tenant was updated
            _mockTenantRepository.Verify(
                repo => repo.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify audit log was created
            _mockAuditLogRepository.Verify(
                repo => repo.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify cache was invalidated
            _mockCacheService.Verify(
                cache => cache.RemoveAsync(It.IsAny<string>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task CreateTenantAsync_ShouldCreateTenant_AndLogAudit()
        {
            // Arrange
            var tenantName = "Test Tenant";
            var tenantHost = "test.domain.com";
            var tenantEmail = "test@example.com";
            var userId = TestHelpers.TestUserId1;
            var createRequest = new CreateTenantRequest
            {
                Name = tenantName,
                Host = tenantHost,
                Email = tenantEmail,
                UserId = userId,
                TimeZone = "UTC"
            };
            
            var tenant = new Tenant
            {
                Name = tenantName,
                Host = tenantHost,
                Email = tenantEmail,
                IsActive = "Y"
            };

            _mockTenantRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _tenantService.CreateTenantAsync(createRequest);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Test Tenant");
            result.Host.ShouldBe("test.domain.com");
            result.Email.ShouldBe("test@example.com");
            result.IsActive.ShouldBe("Y");

            // Verify tenant was added
            _mockTenantRepository.Verify(
                repo => repo.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify audit log was created
            _mockAuditLogRepository.Verify(
                repo => repo.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetTenantByHostAsync_ShouldThrowArgumentException_WhenHostIsNullOrEmpty()
        {
            // Act & Assert
            await Should.ThrowAsync<ArgumentException>(
                async () => await _tenantService.GetTenantByHostAsync(null!));

            await Should.ThrowAsync<ArgumentException>(
                async () => await _tenantService.GetTenantByHostAsync(string.Empty));

            await Should.ThrowAsync<ArgumentException>(
                async () => await _tenantService.GetTenantByHostAsync("   "));
        }

        [Fact]
        public async Task GetTenantByHostAsync_ShouldReturnCachedTenant_WhenTenantExistsInCache()
        {
            // Arrange
            var host = "test.niyacrm.com";
            var normalizedHost = host.Trim().ToLowerInvariant();
            var cacheKey = $"tenant:{normalizedHost}";
            var cachedTenant = new Tenant
            {
                Id = Guid.Parse("00000000-0000-0000-0000-0000000000DC"),
                Name = "Test Tenant",
                Host = normalizedHost,
                Email = "test@example.com",
                IsActive = "Y"
            };

            _mockCacheService
                .Setup(c => c.GetAsync<Tenant>(cacheKey))
                .ReturnsAsync(cachedTenant);

            // Act
            var result = await _tenantService.GetTenantByHostAsync(host);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(cachedTenant);
            
            // Verify cache was checked
            _mockCacheService.Verify(c => c.GetAsync<Tenant>(cacheKey), Times.Once);
            
            // Verify repository was not called
            _mockTenantRepository.Verify(r => r.GetByHostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetTenantByHostAsync_ShouldReturnAndCacheTenant_WhenTenantFoundInRepository()
        {
            // Arrange
            var host = "test.niyacrm.com";
            var normalizedHost = host.Trim().ToLowerInvariant();
            var cacheKey = $"tenant:{normalizedHost}";
            var tenant = new Tenant
            {
                Id = Guid.Parse("00000000-0000-0000-0000-0000000000DD"),
                Name = "Test Tenant",
                Host = normalizedHost,
                Email = "test@example.com",
                IsActive = "Y"
            };

            // Setup cache miss
            _mockCacheService
                .Setup(c => c.GetAsync<Tenant>(cacheKey))
                .ReturnsAsync((Tenant)null!);

            // Setup repository hit
            _mockTenantRepository
                .Setup(r => r.GetByHostAsync(normalizedHost, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            // Act
            var result = await _tenantService.GetTenantByHostAsync(host);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(tenant);
            
            // Verify cache was checked
            _mockCacheService.Verify(c => c.GetAsync<Tenant>(cacheKey), Times.Once);
            
            // Verify repository was called
            _mockTenantRepository.Verify(r => r.GetByHostAsync(normalizedHost, It.IsAny<CancellationToken>()), Times.Once);
            
            // Verify tenant was cached
            _mockCacheService.Verify(c => c.SetAsync(cacheKey, tenant, null, null), Times.Once);
        }

        [Fact]
        public async Task GetTenantByHostAsync_ShouldReturnNull_WhenTenantNotFound()
        {
            // Arrange
            var host = "nonexistent.niyacrm.com";
            var normalizedHost = host.Trim().ToLowerInvariant();
            var cacheKey = $"tenant:{normalizedHost}";

            // Setup cache miss
            _mockCacheService
                .Setup(c => c.GetAsync<Tenant>(cacheKey))
                .ReturnsAsync((Tenant)null!);

            // Setup repository miss
            _mockTenantRepository
                .Setup(r => r.GetByHostAsync(normalizedHost, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant)null!);

            // Act
            var result = await _tenantService.GetTenantByHostAsync(host);

            // Assert
            result.ShouldBeNull();
            
            // Verify cache was checked
            _mockCacheService.Verify(c => c.GetAsync<Tenant>(cacheKey), Times.Once);
            
            // Verify repository was called
            _mockTenantRepository.Verify(r => r.GetByHostAsync(normalizedHost, It.IsAny<CancellationToken>()), Times.Once);
            
            // Verify no caching occurred
            _mockCacheService.Verify(c => c.SetAsync(cacheKey, It.IsAny<Tenant>(), null, null), Times.Never);
        }
    }

    
}

