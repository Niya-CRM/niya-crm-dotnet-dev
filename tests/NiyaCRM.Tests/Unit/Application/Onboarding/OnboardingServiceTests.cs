using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using NiyaCRM.Core.Identity;
using NiyaCRM.Tests.Helpers;
using Moq;
using NiyaCRM.Application.Onboarding;
using NiyaCRM.Core;
using NiyaCRM.Core.Onboarding;
using NiyaCRM.Core.Onboarding.DTOs;
using NiyaCRM.Core.Tenants;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NiyaCRM.Tests.Unit.Application.Onboarding
{
    public class OnboardingServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ITenantService> _mockTenantService;
        private readonly Mock<ILogger<OnboardingService>> _mockLogger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly OnboardingService _onboardingService;

        public OnboardingServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTenantService = new Mock<ITenantService>();
            _mockLogger = new Mock<ILogger<OnboardingService>>();
            _userManager = TestHelpers.MockUserManager();
            _roleManager = TestHelpers.MockRoleManager();

            _onboardingService = new OnboardingService(
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
            var result = await _onboardingService.IsApplicationInstalledAsync();

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
            var result = await _onboardingService.IsApplicationInstalledAsync();

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
                AdminPassword = "Password123!",
                AdminFirstName = "Admin",
                AdminLastName = "User"
            };

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Test Organization",
                Host = "support.organization.com",
                Email = "admin@test.com",
                IsActive = "Y"
            };

            _mockTenantService
                .Setup(service => service.CreateTenantAsync(
                    It.Is<string>(name => name == installationDto.TenantName),
                    It.Is<string>(host => host == installationDto.Host),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
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
            var result = await _onboardingService.InstallApplicationAsync(installationDto);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(tenant.Id);
            result.Name.ShouldBe(tenant.Name);
            result.Host.ShouldBe(tenant.Host);
            
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
                    It.Is<string>(name => name == installationDto.TenantName),
                    It.Is<string>(host => host == installationDto.Host),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
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
                AdminPassword = "Password123!",
                AdminFirstName = "Admin",
                AdminLastName = "User"
            };

            _mockTenantService
                .Setup(service => service.CreateTenantAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
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
            Func<Task> act = async () => await _onboardingService.InstallApplicationAsync(installationDto, CancellationToken.None);
            
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


