using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NiyaCRM.Application.Tenants;
using NiyaCRM.Core;
using NiyaCRM.Core.AuditLogs;
using NiyaCRM.Core.Cache;
using NiyaCRM.Core.Common;
using NiyaCRM.Core.Tenants;
using Shouldly;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NiyaCRM.Tests.Unit.Application.Tenants
{
    public class TenantServiceUpdateTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<TenantService>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ITenantRepository> _mockTenantRepository;
        private readonly Mock<IAuditLogRepository> _mockAuditLogRepository;
        private readonly TenantService _tenantService;
        private readonly string _tenantCachePrefix = "tenant:";

        public TenantServiceUpdateTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLogger = new Mock<ILogger<TenantService>>();
            _mockCacheService = new Mock<ICacheService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
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
                _mockCacheService.Object);
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldThrowArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "";
            var host = "test.domain.com";
            var email = "test@example.com";

            // Act & Assert
            var exception = await Should.ThrowAsync<ArgumentException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, name, host, email));

            exception.Message.ShouldContain("name");
            exception.ParamName.ShouldBe("name");
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldThrowArgumentException_WhenHostIsEmpty()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Tenant";
            var host = "";
            var email = "test@example.com";

            // Act & Assert
            var exception = await Should.ThrowAsync<ArgumentException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, name, host, email));

            exception.Message.ShouldContain("host");
            exception.ParamName.ShouldBe("host");
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Tenant";
            var host = "test.domain.com";
            var email = "";

            // Act & Assert
            var exception = await Should.ThrowAsync<ArgumentException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, name, host, email));

            exception.Message.ShouldContain("email");
            exception.ParamName.ShouldBe("email");
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldThrowInvalidOperationException_WhenTenantNotFound()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Test Tenant";
            var host = "test.domain.com";
            var email = "test@example.com";

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant)null!);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, name, host, email));

            exception.Message.ShouldContain(tenantId.ToString());
            
            // Verify tenant was looked up
            _mockTenantRepository.Verify(
                repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldThrowInvalidOperationException_WhenHostExistsOnDifferentTenant()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var existingTenantId = Guid.NewGuid();
            var name = "Test Tenant";
            var host = "new.domain.com";
            var email = "test@example.com";

            var existingTenant = new Tenant
            {
                Id = existingTenantId,
                Name = "Existing Tenant",
                Host = host.Trim().ToLowerInvariant(),
                Email = "existing@example.com",
                IsActive = true
            };

            var currentTenant = new Tenant
            {
                Id = tenantId,
                Name = "Current Tenant",
                Host = "current.domain.com",
                Email = "current@example.com",
                IsActive = true
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentTenant);

            _mockTenantRepository
                .Setup(repo => repo.GetByHostAsync(host.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTenant);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, name, host, email));

            exception.Message.ShouldContain(host.Trim().ToLowerInvariant());
            
            // Verify tenant was looked up
            _mockTenantRepository.Verify(
                repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verify host was checked
            _mockTenantRepository.Verify(
                repo => repo.GetByHostAsync(host.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldThrowInvalidOperationException_WhenEmailExistsOnDifferentTenant()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var existingTenantId = Guid.NewGuid();
            var name = "Test Tenant";
            var host = "test.domain.com";
            var email = "new@example.com";

            var existingTenant = new Tenant
            {
                Id = existingTenantId,
                Name = "Existing Tenant",
                Host = "existing.domain.com",
                Email = email.Trim().ToLowerInvariant(),
                IsActive = true
            };

            var currentTenant = new Tenant
            {
                Id = tenantId,
                Name = "Current Tenant",
                Host = host,
                Email = "current@example.com",
                IsActive = true
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentTenant);

            _mockTenantRepository
                .Setup(repo => repo.GetByEmailAsync(email.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTenant);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, name, host, email));

            exception.Message.ShouldContain(email.Trim().ToLowerInvariant());
            
            // Verify tenant was looked up
            _mockTenantRepository.Verify(
                repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verify email was checked
            _mockTenantRepository.Verify(
                repo => repo.GetByEmailAsync(email.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldUpdateTenant_AndInvalidateCache_AndLogAudit()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Updated Tenant";
            var host = "updated.domain.com";
            var email = "updated@example.com";
            var databaseName = "updated_db";
            var modifiedBy = "test_user";

            var existingTenant = new Tenant
            {
                Id = tenantId,
                Name = "Original Tenant",
                Host = "original.domain.com",
                Email = "original@example.com",
                DatabaseName = "original_db",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "system"
            };

            var updatedTenant = new Tenant
            {
                Id = tenantId,
                Name = name,
                Host = host.Trim().ToLowerInvariant(),
                Email = email.Trim().ToLowerInvariant(),
                DatabaseName = databaseName,
                IsActive = true,
                CreatedAt = existingTenant.CreatedAt,
                CreatedBy = existingTenant.CreatedBy,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedBy = modifiedBy
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTenant);

            _mockTenantRepository
                .Setup(repo => repo.GetByHostAsync(host.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant)null!);

            _mockTenantRepository
                .Setup(repo => repo.GetByEmailAsync(email.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant)null!);

            _mockTenantRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedTenant);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _tenantService.UpdateTenantAsync(tenantId, name, host, email, databaseName, modifiedBy);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(tenantId);
            result.Name.ShouldBe(name);
            result.Host.ShouldBe(host.Trim().ToLowerInvariant());
            result.Email.ShouldBe(email.Trim().ToLowerInvariant());
            result.DatabaseName.ShouldBe(databaseName);
            result.LastModifiedBy.ShouldBe(modifiedBy);
            
            // Verify tenant was retrieved
            _mockTenantRepository.Verify(
                repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verify host and email uniqueness was checked
            _mockTenantRepository.Verify(
                repo => repo.GetByHostAsync(host.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()),
                Times.Once);
            
            _mockTenantRepository.Verify(
                repo => repo.GetByEmailAsync(email.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verify that cache invalidation happened (without specifying exact keys)
            // The implementation invalidates cache for tenant ID and original host
            _mockCacheService.Verify(
                cache => cache.RemoveAsync(It.IsAny<string>()),
                Times.AtLeast(2));
                
            // Note: The implementation only invalidates cache for tenant ID and original host,
            // not for the new host. This could potentially be an issue in the implementation
            // since the tenant with the new host won't be invalidated in the cache.
            
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
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldNotCheckUniqueness_WhenHostAndEmailUnchanged()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var name = "Updated Tenant";
            var host = "original.domain.com";
            var email = "original@example.com";
            var databaseName = "updated_db";

            var existingTenant = new Tenant
            {
                Id = tenantId,
                Name = "Original Tenant",
                Host = host,
                Email = email,
                DatabaseName = "original_db",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                CreatedBy = "system"
            };

            var updatedTenant = new Tenant
            {
                Id = tenantId,
                Name = name,
                Host = host,
                Email = email,
                DatabaseName = databaseName,
                IsActive = true,
                CreatedAt = existingTenant.CreatedAt,
                CreatedBy = existingTenant.CreatedBy,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedBy = CommonConstant.DEFAULT_USER
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTenant);

            _mockTenantRepository
                .Setup(repo => repo.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedTenant);

            _mockUnitOfWork
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _tenantService.UpdateTenantAsync(tenantId, name, host, email, databaseName);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe(name);
            
            // Verify tenant was retrieved
            _mockTenantRepository.Verify(
                repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verify host and email uniqueness was NOT checked
            _mockTenantRepository.Verify(
                repo => repo.GetByHostAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
            
            _mockTenantRepository.Verify(
                repo => repo.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
            
            // Verify cache was invalidated
            _mockCacheService.Verify(
                cache => cache.RemoveAsync(It.IsAny<string>()),
                Times.AtLeast(2));
            
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
        }
    }
}
