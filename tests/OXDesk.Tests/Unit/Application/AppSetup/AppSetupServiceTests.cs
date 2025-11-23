using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using OXDesk.Core.Identity;
using OXDesk.Tests.Helpers;
using OXDesk.Core.Tenants;
using TenantEntity = OXDesk.Core.Tenants.Tenant;
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
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.DynamicObjects;
using Microsoft.EntityFrameworkCore;
using OXDesk.Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OXDesk.Tests.Unit.Application.AppSetup
{
    public class AppSetupServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITenantService> _mockTenantService;
        private readonly Mock<ILogger<AppSetupService>> _mockLogger;
        private readonly Mock<IValueListService> _mockValueListService;
        private readonly Mock<IValueListItemService> _mockValueListItemService;
        private readonly Mock<IChangeHistoryLogService> _mockChangeHistoryLogService;
        private readonly Mock<ICurrentTenant> _mockCurrentTenant;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IDynamicObjectService> _mockDynamicObjectService;
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
            _mockChangeHistoryLogService = new Mock<IChangeHistoryLogService>();
            _mockCurrentTenant = new Mock<ICurrentTenant>();
            _mockUserService = new Mock<IUserService>();
            _mockDynamicObjectService = new Mock<IDynamicObjectService>();
            _userManager = TestHelpers.MockUserManager();
            _roleManager = TestHelpers.MockRoleManager();

            // Setup mock current tenant with a test tenant ID
            var testTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            _mockCurrentTenant.Setup(ct => ct.Id).Returns(testTenantId);
            _mockCurrentTenant.Setup(ct => ct.Schema).Returns((string?)null);
            _mockCurrentTenant.Setup(ct => ct.ChangeScoped(It.IsAny<Guid?>(), It.IsAny<string?>())).Returns((IDisposable)null!);

            // In-memory TenantDbContext for constructor requirement
            var dbOptions = new DbContextOptionsBuilder<TenantDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            
            // Create fake configuration for hosting model
            var configurationData = new Dictionary<string, string?>
            {
                { "HostingModel", "opensource" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();
            
            var dbContext = new TenantDbContext(dbOptions, _mockCurrentTenant.Object, configuration);

            _mockDynamicObjectService
                .Setup(s => s.GetDynamicObjectIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _AppSetupService = new AppSetupService(
                _mockUnitOfWork.Object,
                _mockTenantService.Object,
                dbContext,
                _userManager,
                _roleManager,
                _mockValueListService.Object,
                _mockValueListItemService.Object,
                _mockChangeHistoryLogService.Object,
                _mockCurrentTenant.Object,
                _mockUserService.Object,
                _mockDynamicObjectService.Object,
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

            var tenantId = Guid.Parse("00000000-0000-0000-0000-00000000012D");
            var tenant = new TenantEntity
            {
                Id = tenantId,
                Name = "Test Organization",
                Host = "support.organization.com",
                Email = "admin@test.com",
                IsActive = "Y"
            };

            // Setup IsApplicationInstalledAsync to return false
            _mockTenantService
                .Setup(service => service.AnyTenantsExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Setup technical user ID
            var technicalUserId = 99;
            _mockUserService
                .Setup(service => service.GetTechnicalUserIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(technicalUserId);

            // Setup with explicit matcher to avoid optional parameters in expression trees
            _mockTenantService
                .Setup(service => service.CreateTenantAsync(
                    It.Is<CreateTenantRequest>(req => 
                        req.Name == setupDto.TenantName && 
                        req.Host == setupDto.Host && 
                        req.Email == setupDto.AdminEmail),
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

            // Setup IsApplicationInstalledAsync to return false
            _mockTenantService
                .Setup(service => service.AnyTenantsExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Setup technical user ID
            var technicalUserId = 99;
            _mockUserService
                .Setup(service => service.GetTechnicalUserIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(technicalUserId);

            _mockTenantService
                .Setup(service => service.CreateTenantAsync(
                    It.Is<CreateTenantRequest>(req => true),
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
