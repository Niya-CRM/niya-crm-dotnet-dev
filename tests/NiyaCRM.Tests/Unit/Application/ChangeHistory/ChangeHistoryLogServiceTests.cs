using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NiyaCRM.Core.ChangeHistory.DTOs;
using Moq;
using NiyaCRM.Application.ChangeHistory;
using NiyaCRM.Core.ChangeHistory;
using Shouldly;
using Xunit;

namespace NiyaCRM.Tests.Unit.Application.ChangeHistory
{
    public class ChangeHistoryLogServiceTests
    {
        private readonly Mock<IChangeHistoryLogRepository> _mockRepository;
        private readonly ChangeHistoryLogService _service;

        public ChangeHistoryLogServiceTests()
        {
            _mockRepository = new Mock<IChangeHistoryLogRepository>();
            _service = new ChangeHistoryLogService(_mockRepository.Object);
        }

        [Fact]
        public async Task CreateChangeHistoryLogAsync_ShouldCreateAndReturnLog()
        {
            // Arrange
            var objectKey = "User";
            var objectItemId = Guid.NewGuid();
            var fieldName = "Email";
            var oldValue = "old@example.com";
            var newValue = "new@example.com";
            var createdBy = Guid.NewGuid();
            
            ChangeHistoryLog? capturedLog = null;
            
            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<ChangeHistoryLog>(), It.IsAny<CancellationToken>()))
                .Callback<ChangeHistoryLog, CancellationToken>((log, _) => capturedLog = log)
                .ReturnsAsync((ChangeHistoryLog log, CancellationToken _) => log);

            // Act
            var result = await _service.CreateChangeHistoryLogAsync(
                objectKey,
                objectItemId,
                fieldName,
                oldValue,
                newValue,
                createdBy);

            // Assert
            result.ShouldNotBeNull();
            capturedLog.ShouldNotBeNull();
            capturedLog!.ObjectKey.ShouldBe(objectKey);
            capturedLog.ObjectItemId.ShouldBe(objectItemId);
            capturedLog.FieldName.ShouldBe(fieldName);
            capturedLog.OldValue.ShouldBe(oldValue);
            capturedLog.NewValue.ShouldBe(newValue);
            capturedLog.CreatedBy.ShouldBe(createdBy);
            
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<ChangeHistoryLog>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetChangeHistoryLogByIdAsync_ShouldReturnLogFromRepository()
        {
            // Arrange
            var logId = Guid.NewGuid();
            var expectedLog = new ChangeHistoryLog
            {
                Id = logId,
                ObjectKey = "User",
                ObjectItemId = Guid.NewGuid(),
                FieldName = "Email",
                OldValue = "old@example.com",
                NewValue = "new@example.com",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid()
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(logId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedLog);

            // Act
            var result = await _service.GetChangeHistoryLogByIdAsync(logId);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedLog);
            _mockRepository.Verify(r => r.GetByIdAsync(logId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetChangeHistoryLogsAsync_ShouldPassFiltersToRepository()
        {
            // Arrange
            var objectKey = "User";
            var objectItemId = Guid.NewGuid();
            var fieldName = "Email";
            var createdBy = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var pageNumber = 2;
            var pageSize = 10;

            var expectedLogs = new List<ChangeHistoryLog>
            {
                new ChangeHistoryLog
                {
                    Id = Guid.NewGuid(),
                    ObjectKey = objectKey,
                    ObjectItemId = objectItemId,
                    FieldName = fieldName,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            _mockRepository
                .Setup(r => r.GetChangeHistoryLogsAsync(
                    objectKey,
                    objectItemId,
                    fieldName,
                    createdBy,
                    startDate,
                    endDate,
                    pageNumber,
                    pageSize,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedLogs);

            // Act
            var query = new ChangeHistoryLogQueryDto
            {
                ObjectKey = objectKey,
                ObjectItemId = objectItemId,
                FieldName = fieldName,
                CreatedBy = createdBy,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var result = await _service.GetChangeHistoryLogsAsync(query);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedLogs);
            
            _mockRepository.Verify(r => r.GetChangeHistoryLogsAsync(
                objectKey,
                objectItemId,
                fieldName,
                createdBy,
                startDate,
                endDate,
                pageNumber,
                pageSize,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllChangeHistoryLogsAsync_ShouldCallRepositoryMethod()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 20;
            var expectedLogs = new List<ChangeHistoryLog>
            {
                new ChangeHistoryLog { Id = Guid.NewGuid() },
                new ChangeHistoryLog { Id = Guid.NewGuid() }
            };

            _mockRepository
                .Setup(r => r.GetAllAsync(
                    pageNumber,
                    pageSize,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedLogs);

            // Act
            var result = await _service.GetAllChangeHistoryLogsAsync(pageNumber, pageSize);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(expectedLogs.Count);
            result.ShouldBe(expectedLogs);
            
            _mockRepository.Verify(r => r.GetAllAsync(
                pageNumber,
                pageSize,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
