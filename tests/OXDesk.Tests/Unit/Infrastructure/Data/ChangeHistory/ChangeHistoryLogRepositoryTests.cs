using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Infrastructure.Data;
using OXDesk.Infrastructure.Data.AuditLogs.ChangeHistory;
using Shouldly;

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

            _dbContext = new ApplicationDbContext(options);
            
            _changeHistoryLogs = new List<ChangeHistoryLog>
            {
                new ChangeHistoryLog(
                    Guid.CreateVersion7(),
                    "User",
                    Guid.CreateVersion7(),
                    "Email",
                    "old@example.com",
                    "new@example.com",
                    Guid.CreateVersion7()
                ),
                new ChangeHistoryLog(
                    Guid.CreateVersion7(),
                    "Contact",
                    Guid.CreateVersion7(),
                    "Phone",
                    "123456789",
                    "987654321",
                    Guid.CreateVersion7()
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
        public async Task GetAllAsync_ShouldReturnAllLogs()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(_changeHistoryLogs.Count);
        }

        [Fact]
        public async Task GetChangeHistoryLogsAsync_WithObjectKeyFilter_ShouldReturnFilteredLogs()
        {
            // Arrange
            var objectKey = "User";

            // Act
            var result = await _repository.GetChangeHistoryLogsAsync(objectKey: objectKey);

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
                Guid.CreateVersion7(),
                "Product",
                Guid.CreateVersion7(),
                "Price",
                "100",
                "150",
                Guid.CreateVersion7()
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
