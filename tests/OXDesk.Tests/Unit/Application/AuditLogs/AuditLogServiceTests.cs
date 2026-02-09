using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OXDesk.Core.AuditLogs;
using OXDesk.Core.AuditLogs.DTOs;
using OXDesk.Core.Common;
using OXDesk.Shared.Services;
using OXDesk.Tests.Helpers;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Application.AuditLogs
{
    public class AuditLogServiceTests
    {
        private readonly Mock<IAuditLogRepository> _mockRepository;
        private readonly Mock<ITraceIdAccessor> _mockTraceIdAccessor;
        private readonly AuditLogService _service;

        public AuditLogServiceTests()
        {
            _mockRepository = new Mock<IAuditLogRepository>();
            _mockTraceIdAccessor = new Mock<ITraceIdAccessor>();
            _mockTraceIdAccessor.Setup(x => x.GetTraceId()).Returns("test-trace-id");
            _service = new AuditLogService(_mockRepository.Object, _mockTraceIdAccessor.Object);
        }

        [Fact]
        public async Task CreateAuditLogAsync_WithGuid_ShouldCreateAndReturnLog()
        {
            // Arrange
            var objectKey = "users";
            var objectItemId = Guid.CreateVersion7();
            var ip = "127.0.0.1";
            var data = "User created";
            var createdBy = TestHelpers.TestUserId1;

            AuditLog? capturedLog = null;

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
                .Callback<AuditLog, CancellationToken>((log, _) => capturedLog = log)
                .ReturnsAsync((AuditLog log, CancellationToken _) => log);

            // Act
            var result = await _service.CreateAuditLogAsync(
                "create",
                objectKey,
                objectItemId,
                ip,
                data,
                createdBy);

            // Assert
            result.ShouldNotBeNull();
            capturedLog.ShouldNotBeNull();
            capturedLog!.ObjectKey.ShouldBe(objectKey);
            capturedLog.ObjectItemIdUuid.ShouldBe(objectItemId);
            capturedLog.ObjectItemIdInt.ShouldBeNull();
            capturedLog.IP.ShouldBe(ip);
            capturedLog.Data.ShouldBe(data);
            capturedLog.CreatedBy.ShouldBe(createdBy);
            capturedLog.TraceId.ShouldBe("test-trace-id");

            _mockRepository.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAuditLogAsync_WithInt_ShouldCreateAndReturnLog()
        {
            // Arrange
            var objectKey = "users";
            var objectItemId = 1001;
            var ip = "192.168.1.1";
            var data = "User updated";
            var createdBy = TestHelpers.TestUserId2;

            AuditLog? capturedLog = null;

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
                .Callback<AuditLog, CancellationToken>((log, _) => capturedLog = log)
                .ReturnsAsync((AuditLog log, CancellationToken _) => log);

            // Act
            var result = await _service.CreateAuditLogAsync(
                "update",
                objectKey,
                objectItemId,
                ip,
                data,
                createdBy);

            // Assert
            result.ShouldNotBeNull();
            capturedLog.ShouldNotBeNull();
            capturedLog!.ObjectKey.ShouldBe(objectKey);
            capturedLog.ObjectItemIdInt.ShouldBe(objectItemId);
            capturedLog.ObjectItemIdUuid.ShouldBeNull();
            capturedLog.IP.ShouldBe(ip);
            capturedLog.Data.ShouldBe(data);
            capturedLog.CreatedBy.ShouldBe(createdBy);
            capturedLog.TraceId.ShouldBe("test-trace-id");

            _mockRepository.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAuditLogByIdAsync_ShouldReturnLogFromRepository()
        {
            // Arrange
            var logId = 123;
            var expectedLog = new AuditLog(
                "login",
                "users",
                1002,
                "10.0.0.1",
                "Login Successful",
                TestHelpers.TestUserId3
            );

            _mockRepository
                .Setup(r => r.GetByIdAsync(logId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedLog);

            // Act
            var result = await _service.GetAuditLogByIdAsync(logId);

            // Assert
            result.ShouldNotBeNull();
            result.ShouldBe(expectedLog);
            _mockRepository.Verify(r => r.GetByIdAsync(logId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAuditLogsAsync_ShouldPassFiltersToRepository()
        {
            // Arrange
            var objectKey = "users";
            var createdBy = TestHelpers.TestUserId1;
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var pageNumber = 1;
            var pageSize = 20;

            var expectedLogs = new List<AuditLog>
            {
                new("login", objectKey, 1001, "127.0.0.1", "Login Successful", createdBy)
                {
                    Id = 456,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            };

            _mockRepository
                .Setup(r => r.GetAuditLogsAsync(
                    It.Is<AuditLogQueryDto>(q =>
                        q.ObjectKey == objectKey &&
                        q.CreatedBy == createdBy &&
                        q.StartDate == startDate &&
                        q.EndDate == endDate &&
                        q.PageNumber == pageNumber &&
                        q.PageSize == pageSize),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedLogs);

            // Act
            var query = new AuditLogQueryDto
            {
                ObjectKey = objectKey,
                CreatedBy = createdBy,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var result = await _service.GetAuditLogsAsync(query);

            // Assert
            result.ShouldNotBeNull();
            var resultList = result.ToList();
            resultList.Count.ShouldBe(expectedLogs.Count);
            var entity = resultList.First();
            entity.ObjectKey.ShouldBe(objectKey);
            entity.CreatedBy.ShouldBe(createdBy);

            _mockRepository.Verify(r => r.GetAuditLogsAsync(
                It.Is<AuditLogQueryDto>(q =>
                    q.ObjectKey == objectKey &&
                    q.CreatedBy == createdBy &&
                    q.StartDate == startDate &&
                    q.EndDate == endDate &&
                    q.PageNumber == pageNumber &&
                    q.PageSize == pageSize),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
