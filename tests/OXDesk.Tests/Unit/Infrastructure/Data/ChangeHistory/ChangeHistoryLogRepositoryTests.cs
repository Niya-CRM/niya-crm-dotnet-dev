using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using OXDesk.Core.Common;
using OXDesk.Core.Tenants;
using OXDesk.DbContext.Data;
using OXDesk.Infrastructure.Data.AuditLogs.ChangeHistory;
using OXDesk.Tests.Helpers;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OXDesk.Tests.Unit.Infrastructure.Data.ChangeHistory
{
    public class ChangeHistoryLogRepositoryTests : IDisposable
    {
        private readonly TenantDbContext _dbContext;
        private readonly ChangeHistoryLogRepository _repository;
        private readonly List<ChangeHistoryLog> _changeHistoryLogs;

        public ChangeHistoryLogRepositoryTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<TenantDbContext>()
                .UseInMemoryDatabase(databaseName: $"ChangeHistoryLogDb_{Guid.CreateVersion7()}")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            // Set up test context with a fake tenant
            var tenantId = Guid.CreateVersion7();
            var currentTenant = new FakeCurrentTenant(tenantId);
            
            // Create fake configuration for hosting model
            var configurationData = new Dictionary<string, string?>
            {
                { "HostingModel", "opensource" }
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();
            
            _dbContext = new TenantDbContext(options, currentTenant, configuration);
            
            _changeHistoryLogs = new List<ChangeHistoryLog>
            {
                new ChangeHistoryLog(
                    1,
                    1001,
                    "Email",
                    "old@example.com",
                    "new@example.com",
                    TestHelpers.TestUserId1
                ),
                new ChangeHistoryLog(
                    2,
                    1002,
                    "Phone",
                    "123456789",
                    "987654321",
                    TestHelpers.TestUserId2
                )
            };

            // Add test data to the in-memory database
            _dbContext.ChangeHistoryLogs.AddRange(_changeHistoryLogs);
            _dbContext.SaveChanges();
            
            _repository = new ChangeHistoryLogRepository(_dbContext);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectLog()
        {
            // Arrange
            var targetLog = _changeHistoryLogs.First();

            // Act
            var result = await _repository.GetByIdAsync(targetLog.Id);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(targetLog.Id);
            result.ObjectId.ShouldBe(targetLog.ObjectId);
            result.FieldName.ShouldBe(targetLog.FieldName);
        }

        [Fact]
        public async Task GetChangeHistoryLogsAsync_WithObjectIdFilter_ShouldReturnFilteredLogs()
        {
            var objectId = 1;

            // Act
            var query = new ChangeHistoryLogQueryDto
            {
                ObjectId = objectId
            };
            var result = await _repository.GetChangeHistoryLogsAsync(query);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.First().ObjectId.ShouldBe(objectId);
        }

        [Fact]
        public async Task AddAsync_ShouldAddLogToDbSet()
        {
            // Arrange
            var newLog = new ChangeHistoryLog(
                3,
                1003,
                "Price",
                "100",
                "150",
                TestHelpers.TestUserId1
            );

            // Act
            var result = await _repository.AddAsync(newLog);

            // Assert
            result.ShouldBe(newLog);
            
            // Verify it was added to the database
            var savedLog = await _dbContext.ChangeHistoryLogs.FindAsync(newLog.Id);
            savedLog.ShouldNotBeNull();
            savedLog.Id.ShouldBe(newLog.Id);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

namespace OXDesk.Tests.Unit.Infrastructure.Data.ChangeHistory
{
    // Minimal test double for ICurrentTenant
    internal sealed class FakeCurrentTenant : ICurrentTenant
    {
        private Guid? _id;
        private string? _schema;
        
        public Guid? Id => _id;
        public string? Schema => _schema;

        public FakeCurrentTenant(Guid? tenantId)
        {
            _id = tenantId;
            _schema = null;
        }

        public void Change(Guid? tenantId, string? schema = null)
        {
            _id = tenantId;
            _schema = schema;
        }

        public IDisposable ChangeScoped(Guid? tenantId, string? schema = null)
        {
            var previousId = _id;
            var previousSchema = _schema;
            _id = tenantId;
            _schema = schema;
            return new Restore(() =>
            {
                _id = previousId;
                _schema = previousSchema;
            });
        }

        private sealed class Restore : IDisposable
        {
            private readonly Action _onDispose;
            public Restore(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }
    }
}
