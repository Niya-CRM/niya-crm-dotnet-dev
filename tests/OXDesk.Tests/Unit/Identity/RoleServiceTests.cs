using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;
using OXDesk.Core.Common;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using OXDesk.Identity.Services;

namespace OXDesk.Tests.Unit.Identity
{
    public interface IQueryableRoleStoreComposite : IQueryableRoleStore<ApplicationRole> { }

    public class RoleServiceTests
    {
        private readonly Mock<IQueryableRoleStoreComposite> _mockRoleStore;
        private readonly Mock<IRoleClaimRepository> _mockRoleClaimRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly RoleService _roleService;

        public RoleServiceTests()
        {
            _mockRoleStore = new Mock<IQueryableRoleStoreComposite>();
            _mockRoleClaimRepository = new Mock<IRoleClaimRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();

            _mockCurrentUser.Setup(u => u.Id).Returns(10001);

            // Default: empty queryable roles
            _mockRoleStore.Setup(s => s.Roles)
                .Returns(new List<ApplicationRole>().AsQueryable());

            // Default: CreateAsync succeeds
            _mockRoleStore
                .Setup(s => s.CreateAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityResult.Success);

            // Default: UpdateAsync succeeds
            _mockRoleStore
                .Setup(s => s.UpdateAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(IdentityResult.Success);

            var roleValidators = new List<IRoleValidator<ApplicationRole>>();
            var keyNormalizer = new Mock<ILookupNormalizer>();
            keyNormalizer.Setup(n => n.NormalizeName(It.IsAny<string>()))
                .Returns<string>(name => name?.ToUpperInvariant() ?? string.Empty);
            var errors = new Mock<IdentityErrorDescriber>();
            var logger = new Mock<ILogger<RoleManager<ApplicationRole>>>();

            _roleManager = new RoleManager<ApplicationRole>(
                _mockRoleStore.Object,
                roleValidators,
                keyNormalizer.Object,
                errors.Object,
                logger.Object);

            _roleService = new RoleService(
                _roleManager,
                _mockRoleClaimRepository.Object,
                _mockCurrentUser.Object);
        }

        [Fact]
        public async Task CreateRoleAsync_SetsDescription()
        {
            // Arrange
            var request = new CreateRoleRequest
            {
                Name = "TestRole",
                Description = "A test role description"
            };

            // Act
            var result = await _roleService.CreateRoleAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Description.ShouldBe("A test role description");
        }

        [Fact]
        public async Task CreateRoleAsync_SetsNullDescription_WhenNotProvided()
        {
            // Arrange
            var request = new CreateRoleRequest
            {
                Name = "TestRole"
            };

            // Act
            var result = await _roleService.CreateRoleAsync(request);

            // Assert
            result.ShouldNotBeNull();
            result.Description.ShouldBeNull();
        }

        [Fact]
        public async Task UpdateRoleAsync_UpdatesDescription()
        {
            // Arrange
            var existingRole = new ApplicationRole("ExistingRole")
            {
                Id = 10001,
                Description = "Old description",
                CreatedBy = 10001,
                UpdatedBy = 10001
            };

            _mockRoleStore
                .Setup(s => s.FindByIdAsync("10001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRole);

            // Return empty queryable so duplicate check passes
            _mockRoleStore.Setup(s => s.Roles)
                .Returns(new List<ApplicationRole> { existingRole }.AsQueryable());

            var request = new UpdateRoleRequest
            {
                Name = "ExistingRole",
                Description = "New description"
            };

            // Act
            var result = await _roleService.UpdateRoleAsync(10001, request);

            // Assert
            result.ShouldNotBeNull();
            result.Description.ShouldBe("New description");
        }

        [Fact]
        public async Task UpdateRoleAsync_ClearsDescription_WhenNull()
        {
            // Arrange
            var existingRole = new ApplicationRole("ExistingRole")
            {
                Id = 10001,
                Description = "Old description",
                CreatedBy = 10001,
                UpdatedBy = 10001
            };

            _mockRoleStore
                .Setup(s => s.FindByIdAsync("10001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingRole);

            _mockRoleStore.Setup(s => s.Roles)
                .Returns(new List<ApplicationRole> { existingRole }.AsQueryable());

            var request = new UpdateRoleRequest
            {
                Name = "ExistingRole",
                Description = null
            };

            // Act
            var result = await _roleService.UpdateRoleAsync(10001, request);

            // Assert
            result.ShouldNotBeNull();
            result.Description.ShouldBeNull();
        }
    }
}
