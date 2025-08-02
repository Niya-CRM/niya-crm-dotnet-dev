using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using NiyaCRM.Core.Identity;
using NiyaCRM.Tests.Helpers;
using NiyaCRM.Core.Tenants;
using NiyaCRM.Core.Tenants.DTOs;
using NiyaCRM.AppInstallation.Services;
using NiyaCRM.Core.AppInstallation.AppSetup.DTOs;
using NiyaCRM.Infrastructure.Data;
using NiyaCRM.Core;
using Moq;
using Shouldly;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Reflection;

namespace NiyaCRM.Tests.Unit.Application.ApplicationSetup
{
    public class ApplicationSetupServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITenantService> _mockTenantService;
        private readonly Mock<ILogger<ApplicationSetupService>> _mockLogger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationSetupService _applicationSetupService;

        public ApplicationSetupServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTenantService = new Mock<ITenantService>();
            _mockLogger = new Mock<ILogger<ApplicationSetupService>>();
            _userManager = TestHelpers.MockUserManager();
            _roleManager = TestHelpers.MockRoleManager();

            _applicationSetupService = new ApplicationSetupService(
                _mockUnitOfWork.Object,
                _mockTenantService.Object,
                _userManager,
                _roleManager,
                _mockLogger.Object);
        }

        [Fact]
        public async Task IsApplicationInstalledAsync_ShouldReturnTrue_WhenTenantsExist()
        {
            // Arrange
            _mockTenantService
                .Setup(service => service.AnyTenantsExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _applicationSetupService.IsApplicationInstalledAsync();

            // Assert
            result.ShouldBeTrue();
            
            // Verify tenant service was called
            _mockTenantService.Verify(
                service => service.AnyTenantsExistAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task IsApplicationInstalledAsync_ShouldReturnFalse_WhenNoTenantsExist()
        {
            // Arrange
            _mockTenantService
                .Setup(service => service.AnyTenantsExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _applicationSetupService.IsApplicationInstalledAsync();

            // Assert
            result.ShouldBeFalse();
            
            // Verify tenant service was called
            _mockTenantService.Verify(
                service => service.AnyTenantsExistAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task InstallApplicationAsync_ShouldCreateTenant_UsingTenantService()
        {
            // Arrange
            var installationDto = new AppInstallationDto
            {
                TenantName = "Test Organization",
                Host = "support.organization.com",
                AdminEmail = "admin@test.com",
                Password = "Password123!",
                FirstName = "Admin",
                LastName = "User"
            };

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Test Organization",
                Host = "support.organization.com",
                Email = "admin@test.com",
                IsActive = "Y"
            };

            // Setup with explicit matcher to avoid optional parameters in expression trees
            _mockTenantService
                .Setup(service => service.CreateTenantAsync(
                    It.Is<CreateTenantRequest>(req => 
                        req.Name == installationDto.TenantName && 
                        req.Host == installationDto.Host && 
                        req.Email == installationDto.AdminEmail),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            _mockUnitOfWork
                .Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _applicationSetupService.InstallApplicationAsync(installationDto);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(tenant);
            
            // Verify transaction was used
            _mockUnitOfWork.Verify(
                uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);
                
            _mockUnitOfWork.Verify(
                uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verify tenant service was used to create tenant
            _mockTenantService.Verify(
                service => service.CreateTenantAsync(
                    It.Is<CreateTenantRequest>(req => true),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task InstallApplicationAsync_ShouldRollbackTransaction_WhenExceptionOccurs()
        {
            // Arrange
            var installationDto = new AppInstallationDto
            {
                TenantName = "Test Organization",
                Host = "test-organization",
                AdminEmail = "admin@test.com",
                Password = "Password123!",
                FirstName = "Admin",
                LastName = "User"
            };

            _mockTenantService
                .Setup(service => service.CreateTenantAsync(
                    It.Is<CreateTenantRequest>(req => true),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            _mockUnitOfWork
                .Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork
                .Setup(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _applicationSetupService.InstallApplicationAsync(installationDto);
            
            // Assert
            await act.ShouldThrowAsync<Exception>();
            
            // Verify transaction was rolled back
            _mockUnitOfWork.Verify(
                uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);
                
            _mockUnitOfWork.Verify(
                uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
