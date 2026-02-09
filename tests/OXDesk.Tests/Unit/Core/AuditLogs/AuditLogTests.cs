using System;
using OXDesk.Core.AuditLogs;
using OXDesk.Tests.Helpers;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Core.AuditLogs
{
    public class AuditLogTests
    {
        private const string TestObjectKey = "users";

        [Fact]
        public void ParameterizedConstructor_WithGuid_ShouldInitializeProperties()
        {
            // Arrange
            var objectKey = TestObjectKey;
            var objectItemId = Guid.CreateVersion7();
            var ip = "127.0.0.1";
            var data = "User created";
            var createdBy = TestHelpers.TestUserId1;

            // Act
            var auditLog = new AuditLog(
                "create",
                objectKey,
                objectItemId,
                ip,
                data,
                createdBy);

            // Assert
            auditLog.Id.ShouldBe(0);
            auditLog.Event.ShouldBe("create");
            auditLog.ObjectKey.ShouldBe(objectKey);
            auditLog.ObjectItemIdUuid.ShouldBe(objectItemId);
            auditLog.ObjectItemIdInt.ShouldBeNull();
            auditLog.IP.ShouldBe(ip);
            auditLog.Data.ShouldBe(data);
            auditLog.CreatedAt.ShouldNotBe(default(DateTime));
            (DateTime.UtcNow - auditLog.CreatedAt).TotalSeconds.ShouldBeLessThan(1);
            auditLog.CreatedBy.ShouldBe(createdBy);
        }

        [Fact]
        public void ParameterizedConstructor_WithInt_ShouldInitializeProperties()
        {
            // Arrange
            var objectKey = TestObjectKey;
            var objectItemId = 1001;
            var ip = "192.168.1.1";
            var data = "User updated";
            var createdBy = TestHelpers.TestUserId2;

            // Act
            var auditLog = new AuditLog(
                "update",
                objectKey,
                objectItemId,
                ip,
                data,
                createdBy);

            // Assert
            auditLog.Id.ShouldBe(0);
            auditLog.Event.ShouldBe("update");
            auditLog.ObjectKey.ShouldBe(objectKey);
            auditLog.ObjectItemIdInt.ShouldBe(objectItemId);
            auditLog.ObjectItemIdUuid.ShouldBeNull();
            auditLog.IP.ShouldBe(ip);
            auditLog.Data.ShouldBe(data);
            auditLog.CreatedAt.ShouldNotBe(default(DateTime));
            (DateTime.UtcNow - auditLog.CreatedAt).TotalSeconds.ShouldBeLessThan(1);
            auditLog.CreatedBy.ShouldBe(createdBy);
        }

        [Fact]
        public void ParameterizedConstructor_WithNullData_ShouldInitializeProperties()
        {
            // Arrange
            var objectKey = TestObjectKey;
            var objectItemId = 1002;
            var ip = "10.0.0.1";
            var createdBy = TestHelpers.TestUserId3;

            // Act
            var auditLog = new AuditLog(
                "delete",
                objectKey,
                objectItemId,
                ip,
                null,
                createdBy);

            // Assert
            auditLog.ObjectKey.ShouldBe(objectKey);
            auditLog.ObjectItemIdInt.ShouldBe(objectItemId);
            auditLog.Data.ShouldBeNull();
            auditLog.CreatedBy.ShouldBe(createdBy);
        }

        [Fact]
        public void ParameterizedConstructor_ShouldSetCreatedAtToUtcNow()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;

            // Act
            var auditLog = new AuditLog(
                "login",
                TestObjectKey,
                1003,
                "127.0.0.1",
                "Login Successful",
                TestHelpers.TestUserId1);

            var afterCreation = DateTime.UtcNow;

            // Assert
            auditLog.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreation);
            auditLog.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreation);
        }

        [Fact]
        public void DefaultConstructor_ShouldInitializeDefaults()
        {
            // Act
            var auditLog = new AuditLog();

            // Assert
            auditLog.Id.ShouldBe(0);
            auditLog.Event.ShouldBe(string.Empty);
            auditLog.ObjectKey.ShouldBe(string.Empty);
            auditLog.ObjectItemIdUuid.ShouldBeNull();
            auditLog.ObjectItemIdInt.ShouldBeNull();
            auditLog.IP.ShouldBe(string.Empty);
            auditLog.Data.ShouldBeNull();
            auditLog.TraceId.ShouldBeNull();
        }
    }
}
