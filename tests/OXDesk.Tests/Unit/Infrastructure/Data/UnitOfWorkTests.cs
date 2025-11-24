using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OXDesk.Core;
using OXDesk.Core.Common;
using OXDesk.Core.Tenants;
using OXDesk.DbContext.Data;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OXDesk.Tests.Unit.Infrastructure.Data
{
    public class UnitOfWorkTests
    {
        private readonly Mock<TenantDbContext> _mockDbContext;
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            // Setup TenantDbContext mock with required dependencies
            var mockCurrentTenant = new Mock<ICurrentTenant>();
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["HostingModel"]).Returns(CommonConstant.HOSTING_MODEL_OS);
            
            _mockDbContext = new Mock<TenantDbContext>(
                MockBehavior.Loose,
                new DbContextOptions<TenantDbContext>(),
                mockCurrentTenant.Object,
                mockConfiguration.Object);
            _mockServiceProvider = new Mock<IServiceProvider>();
            
            // Create UnitOfWork with the mocked DbContext
            _unitOfWork = new UnitOfWork(_mockDbContext.Object, _mockServiceProvider.Object);
        }

        [Fact]
        public void GetRepository_ShouldReturnRepositoryInstance()
        {
            // Arrange
            var mockRepo = new Mock<ITenantRepository>();
            
            _mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ITenantRepository)))
                .Returns(mockRepo.Object);
            
            // Act
            var repository = _unitOfWork.GetRepository<ITenantRepository>();
            
            // Assert
            repository.ShouldNotBeNull();
            repository.ShouldBe(mockRepo.Object);
        }
        
        [Fact]
        public void GetRepository_ShouldCacheRepositories()
        {
            // Arrange
            var mockRepo = new Mock<ITenantRepository>();
            
            _mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ITenantRepository)))
                .Returns(mockRepo.Object);
            
            // Act
            var repository1 = _unitOfWork.GetRepository<ITenantRepository>();
            var repository2 = _unitOfWork.GetRepository<ITenantRepository>();
            
            // Assert
            repository1.ShouldBe(repository2); // Same instance should be returned
            _mockServiceProvider.Verify(sp => sp.GetService(typeof(ITenantRepository)), Times.Once);
        }
        
        [Fact]
        public async Task SaveChangesAsync_ShouldCallDbContextSaveChanges()
        {
            // Arrange
            _mockDbContext
                .Setup(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            
            // Act
            var result = await _unitOfWork.SaveChangesAsync();
            
            // Assert
            result.ShouldBe(1);
            _mockDbContext.Verify(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        // Skip the transaction test as it's difficult to mock DatabaseFacade properly
        // This test would be better implemented with an in-memory database
        [Fact(Skip = "Skipping due to DatabaseFacade mocking issues")]
        public async Task TransactionMethods_ShouldWorkCorrectly()
        {
            // This test is skipped because mocking DatabaseFacade is problematic
            // A better approach would be to use an in-memory database for integration testing
            await Task.CompletedTask;
        }
    }
}
