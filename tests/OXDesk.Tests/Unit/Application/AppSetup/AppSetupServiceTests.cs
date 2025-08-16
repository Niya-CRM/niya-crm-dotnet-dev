using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using OXDesk.Core.Identity;
using OXDesk.Tests.Helpers;
using OXDesk.Core.Tenants;
using OXDesk.Core.Tenants.DTOs;
using OXDesk.AppInstallation.Services;
using OXDesk.Core.AppInstallation.AppSetup.DTOs;
using OXDesk.Infrastructure.Data;
using OXDesk.Core;
using Moq;
using Shouldly;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Reflection;
using OXDesk.Core.ValueLists;

namespace OXDesk.Tests.Unit.Application.AppSetup
{
    public class AppSetupServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITenantService> _mockTenantService;
        private readonly Mock<ILogger<AppSetupService>> _mockLogger;
        private readonly Mock<IValueListService> _mockValueListService;
        private readonly Mock<IValueListItemService> _mockValueListItemService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly AppSetupService _AppSetupService;

        public AppSetupServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTenantService = new Mock<ITenantService>();
            _mockLogger = new Mock<ILogger<AppSetupService>>();
            _mockValueListService = new Mock<IValueListService>();
            _mockValueListItemService = new Mock<IValueListItemService>();
            _userManager = TestHelpers.MockUserManager();
            _roleManager = TestHelpers.MockRoleManager();

            _AppSetupService = new AppSetupService(
                _mockUnitOfWork.Object,
                _mockTenantService.Object,
                _userManager,
                _roleManager,
                _mockValueListService.Object,
                _mockValueListItemService.Object,
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
            var result = await _AppSetupService.IsApplicationInstalledAsync();

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
            var result = await _AppSetupService.IsApplicationInstalledAsync();

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
            var setupDto = new AppSetupDto
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
                Id = Guid.CreateVersion7(),
                Name = "Test Organization",
                Host = "support.organization.com",
                Email = "admin@test.com",
                IsActive = "Y"
            };

            // Setup with explicit matcher to avoid optional parameters in expression trees
            _mockTenantService
                .Setup(service => service.CreateTenantAsync(
                    It.Is<CreateTenantRequest>(req => 
                        req.Name == setupDto.TenantName && 
                        req.Host == setupDto.Host && 
                        req.Email == setupDto.AdminEmail),
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
            var result = await _AppSetupService.InstallApplicationAsync(setupDto);

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
            var setupDto = new AppSetupDto
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
            Func<Task> act = async () => await _AppSetupService.InstallApplicationAsync(setupDto);
            
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
