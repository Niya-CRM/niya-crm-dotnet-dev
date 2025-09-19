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
using Shouldly;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using OXDesk.Application.Common;

namespace OXDesk.Tests.Unit.Application.Tenants
{
    public class TenantServiceUpdateTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILogger<TenantService>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ICurrentTenant> _mockCurrentTenant;
        private readonly Mock<ITenantRepository> _mockTenantRepository;
        private readonly Mock<IAuditLogRepository> _mockAuditLogRepository;
        private readonly TenantService _tenantService;

        public TenantServiceUpdateTests()
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
        public async Task UpdateTenantAsync_ShouldThrowValidationException_WhenNameIsEmpty()
        {
            // Arrange
            var tenantId = 1;
            var updateRequest = new UpdateTenantRequest
            {
                Name = "",
                Host = "test.domain.com",
                Email = "test@example.com",
                UserId = Guid.CreateVersion7(),
                TimeZone = "UTC"
            };

            // Act & Assert
            var exception = await Should.ThrowAsync<ValidationException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, updateRequest));

            exception.Message.ShouldContain("Name");
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldThrowValidationException_WhenHostIsEmpty()
        {
            // Arrange
            var tenantId = 2;
            var updateRequest = new UpdateTenantRequest
            {
                Name = "Test Tenant",
                Host = "",
                Email = "test@example.com",
                UserId = Guid.CreateVersion7(),
                TimeZone = "UTC"
            };

            // Act & Assert
            var exception = await Should.ThrowAsync<ValidationException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, updateRequest));

            exception.Message.ShouldContain("Host");
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldThrowValidationException_WhenEmailIsEmpty()
        {
            // Arrange
            var tenantId = 3;
            var updateRequest = new UpdateTenantRequest
            {
                Name = "Test Tenant",
                Host = "test.domain.com",
                Email = "",
                UserId = Guid.CreateVersion7(),
                TimeZone = "UTC"
            };

            // Act & Assert
            var exception = await Should.ThrowAsync<ValidationException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, updateRequest));

            exception.Message.ShouldContain("Email");
        }

        [Fact]
        public async Task UpdateTenantAsync_ShouldThrowInvalidOperationException_WhenTenantNotFound()
        {
            // Arrange
            var tenantId = 4;
            var updateRequest = new UpdateTenantRequest
            {
                Name = "Test Tenant",
                Host = "test.domain.com",
                Email = "test@example.com",
                UserId = Guid.CreateVersion7(),
                TimeZone = "UTC"
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Tenant)null!);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, updateRequest));

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
            var tenantId = 5;
            var existingTenantId = 6;
            var host = "new.domain.com";
            var updateRequest = new UpdateTenantRequest
            {
                Name = "Test Tenant",
                Host = host,
                Email = "test@example.com",
                UserId = Guid.CreateVersion7(),
                TimeZone = "UTC"
            };

            var existingTenant = new Tenant
            {
                Id = existingTenantId,
                Name = "Existing Tenant",
                Host = host.Trim().ToLowerInvariant(),
                Email = "existing@example.com",
                IsActive = "Y"
            };

            var currentTenant = new Tenant
            {
                Id = tenantId,
                Name = "Current Tenant",
                Host = "current.domain.com",
                Email = "current@example.com",
                IsActive = "Y"
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentTenant);

            _mockTenantRepository
                .Setup(repo => repo.GetByHostAsync(host.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTenant);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, updateRequest));

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
            var tenantId = 7;
            var existingTenantId = 8;
            var host = "test.domain.com";
            var email = "new@example.com";
            var updateRequest = new UpdateTenantRequest
            {
                Name = "Test Tenant",
                Host = host,
                Email = email,
                UserId = Guid.CreateVersion7(),
                TimeZone = "UTC"
            };

            var existingTenant = new Tenant
            {
                Id = existingTenantId,
                Name = "Existing Tenant",
                Host = "existing.domain.com",
                Email = email.Trim().ToLowerInvariant(),
                IsActive = "Y"
            };

            var currentTenant = new Tenant
            {
                Id = tenantId,
                Name = "Current Tenant",
                Host = host,
                Email = "current@example.com",
                IsActive = "Y"
            };

            _mockTenantRepository
                .Setup(repo => repo.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(currentTenant);

            _mockTenantRepository
                .Setup(repo => repo.GetByEmailAsync(email.Trim().ToLowerInvariant(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingTenant);

            // Act & Assert
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                async () => await _tenantService.UpdateTenantAsync(tenantId, updateRequest));

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
        public async Task UpdateTenantAsync_ShouldNotCheckUniqueness_WhenHostAndEmailUnchanged()
        {
            // Arrange
            var tenantId = 10;
            var name = "Updated Tenant";
            var host = "original.domain.com";
            var email = "original@example.com";
            var databaseName = "updated_db";
            var userId = Guid.CreateVersion7();
            
            var updateRequest = new UpdateTenantRequest
            {
                Name = name,
                Host = host,
                Email = email,
                UserId = userId,
                TimeZone = "UTC"
            };

            var existingTenant = new Tenant
            {
                Id = tenantId,
                Name = "Original Tenant",
                Host = host,
                Email = email,
                DatabaseName = "original_db",
                IsActive = "Y",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
            };

            var updatedTenant = new Tenant
            {
                Id = tenantId,
                Name = name,
                Host = host,
                Email = email,
                DatabaseName = databaseName,
                IsActive = "Y",
                CreatedAt = existingTenant.CreatedAt,
                CreatedBy = existingTenant.CreatedBy,
                LastModifiedAt = DateTime.UtcNow,
                LastModifiedBy = CommonConstant.DEFAULT_SYSTEM_USER
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
            var result = await _tenantService.UpdateTenantAsync(tenantId, updateRequest);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(tenantId);
            result.Name.ShouldBe(name);
            result.Host.ShouldBe(host);
            result.Email.ShouldBe(email);
            result.DatabaseName.ShouldBe(databaseName);
            
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




