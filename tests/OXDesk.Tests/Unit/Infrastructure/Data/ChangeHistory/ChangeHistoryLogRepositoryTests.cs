using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using OXDesk.Infrastructure.Data;
using OXDesk.Infrastructure.Data.AuditLogs.ChangeHistory;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using OXDesk.Core.Tenants;

namespace OXDesk.Tests.Unit.Infrastructure.Data.ChangeHistory
{
    public class ChangeHistoryLogRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ChangeHistoryLogRepository _repository;
        private readonly List<ChangeHistoryLog> _changeHistoryLogs;

        public ChangeHistoryLogRepositoryTests()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"ChangeHistoryLogDb_{Guid.CreateVersion7()}")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            // Set a deterministic tenant for this test context so global filters and ApplyTenantId work consistently
            var tenantId = 1001;
            var services = new ServiceCollection().BuildServiceProvider();
            var currentTenant = new FakeCurrentTenant(tenantId);
            _dbContext = new ApplicationDbContext(options, services, currentTenant);
            
            _changeHistoryLogs = new List<ChangeHistoryLog>
            {
                new ChangeHistoryLog(
                    "User",
                    101,
                    "Email",
                    "old@example.com",
                    "new@example.com",
                    10001
                ),
                new ChangeHistoryLog(
                    "Contact",
                    102,
                    "Phone",
                    "123456789",
                    "987654321",
                    10002
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
            result.ObjectKey.ShouldBe(targetLog.ObjectKey);
            result.FieldName.ShouldBe(targetLog.FieldName);
        }

        [Fact]
        public async Task GetChangeHistoryLogsAsync_WithObjectKeyFilter_ShouldReturnFilteredLogs()
        {
            // Arrange
            var objectKey = "User";

            // Act
            var query = new ChangeHistoryLogQueryDto
            {
                ObjectKey = objectKey,
                ObjectItemId = 0
            };
            var result = await _repository.GetChangeHistoryLogsAsync(query);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(1);
            result.First().ObjectKey.ShouldBe(objectKey);
        }

        [Fact]
        public async Task AddAsync_ShouldAddLogToDbSet()
        {
            // Arrange
            var newLog = new ChangeHistoryLog(
                "Product",
                201,
                "Price",
                "100",
                "150",
                10001
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
        private int? _id;
        public int? Id => _id;

        public FakeCurrentTenant(int? tenantId)
        {
            _id = tenantId;
        }

        public void Change(int? tenantId)
        {
            _id = tenantId;
        }

        public IDisposable ChangeScoped(int? tenantId)
        {
            var previous = _id;
            _id = tenantId;
            return new Restore(() => _id = previous);
        }

        private sealed class Restore : IDisposable
        {
            private readonly Action _onDispose;
            public Restore(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }
    }
}
