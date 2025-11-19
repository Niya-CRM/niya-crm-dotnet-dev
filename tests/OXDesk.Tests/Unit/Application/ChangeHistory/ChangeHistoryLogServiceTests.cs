using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OXDesk.Application.AuditLogs.ChangeHistory;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.AuditLogs.ChangeHistory.DTOs;
using OXDesk.Tests.Helpers;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Application.ChangeHistory
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
            var objectItemId = Guid.Parse("00000000-0000-0000-0000-0000000003E9"); // 1001
            var fieldName = "Email";
            var oldValue = "old@example.com";
            var newValue = "new@example.com";
            var createdBy = TestHelpers.TestUserId1;
            
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
            capturedLog.ObjectItemIdUuid.ShouldBe(objectItemId);
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
            var logId = 123;
            var expectedLog = new ChangeHistoryLog(
                "User",
                Guid.Parse("00000000-0000-0000-0000-0000000003EA"),
                "Email",
                "old@example.com",
                "new@example.com",
                TestHelpers.TestUserId2
            );

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
            var objectItemId = Guid.Parse("00000000-0000-0000-0000-0000000003EB"); // 1003
            var fieldName = "Email";
            var createdBy = TestHelpers.TestUserId3;
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var pageNumber = 2;
            var pageSize = 10;

            var expectedLogs = new List<ChangeHistoryLog>
            {
                new(objectKey, objectItemId, fieldName, null, null, createdBy) {
                    Id = 456,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            _mockRepository
                .Setup(r => r.GetChangeHistoryLogsAsync(
                    It.Is<ChangeHistoryLogQueryDto>(q =>
                        q.ObjectKey == objectKey &&
                        q.ObjectItemIdUuid == objectItemId &&
                        q.FieldName == fieldName &&
                        q.CreatedBy == createdBy &&
                        q.StartDate == startDate &&
                        q.EndDate == endDate &&
                        q.PageNumber == pageNumber &&
                        q.PageSize == pageSize),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedLogs);

            // Act
            var query = new ChangeHistoryLogQueryDto
            {
                ObjectKey = objectKey,
                ObjectItemIdUuid = objectItemId,
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
            result.Count().ShouldBe(expectedLogs.Count);
            var entity = result.First();
            entity.ObjectKey.ShouldBe(objectKey);
            entity.ObjectItemIdUuid.ShouldBe(objectItemId);
            entity.FieldName.ShouldBe(fieldName);
            entity.CreatedBy.ShouldBe(createdBy);
            
            _mockRepository.Verify(r => r.GetChangeHistoryLogsAsync(
                It.Is<ChangeHistoryLogQueryDto>(q =>
                    q.ObjectKey == objectKey &&
                    q.ObjectItemIdUuid == objectItemId &&
                    q.FieldName == fieldName &&
                    q.CreatedBy == createdBy &&
                    q.StartDate == startDate &&
                    q.EndDate == endDate &&
                    q.PageNumber == pageNumber &&
                    q.PageSize == pageSize),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
