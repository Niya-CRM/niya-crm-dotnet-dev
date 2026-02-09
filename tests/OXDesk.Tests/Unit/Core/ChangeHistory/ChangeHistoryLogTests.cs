using System;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Tests.Helpers;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Core.ChangeHistory
{
    public class ChangeHistoryLogTests
    {
        private const string TestObjectKey = "users";

        [Fact]
        public void ParameterizedConstructor_WithInt_ShouldInitializeProperties()
        {
            // Arrange
            var objectKey = TestObjectKey;
            var objectItemId = 1001;
            var fieldName = "Email";
            var oldValue = "old@example.com";
            var newValue = "new@example.com";
            var createdBy = TestHelpers.TestUserId1;

            // Act
            var changeHistoryLog = new ChangeHistoryLog(
                objectKey,
                objectItemId,
                fieldName,
                oldValue,
                newValue,
                createdBy);

            // Assert
            changeHistoryLog.Id.ShouldBe(0);
            changeHistoryLog.ObjectKey.ShouldBe(objectKey);
            changeHistoryLog.ObjectItemIdInt.ShouldBe(objectItemId);
            changeHistoryLog.ObjectItemIdUuid.ShouldBeNull();
            changeHistoryLog.FieldName.ShouldBe(fieldName);
            changeHistoryLog.OldValue.ShouldBe(oldValue);
            changeHistoryLog.NewValue.ShouldBe(newValue);
            changeHistoryLog.CreatedAt.ShouldNotBe(default(DateTime));
            // CreatedAt should be within the last second (allowing for test execution time)
            (DateTime.UtcNow - changeHistoryLog.CreatedAt).TotalSeconds.ShouldBeLessThan(1);
            changeHistoryLog.CreatedBy.ShouldBe(createdBy);
        }

        [Fact]
        public void ParameterizedConstructor_WithGuid_ShouldInitializeProperties()
        {
            // Arrange
            var objectItemId = Guid.CreateVersion7();
            var fieldName = "Email";
            var oldValue = "old@example.com";
            var newValue = "new@example.com";
            var createdBy = TestHelpers.TestUserId1;

            // Act
            var changeHistoryLog = new ChangeHistoryLog(
                TestObjectKey,
                objectItemId,
                fieldName,
                oldValue,
                newValue,
                createdBy);

            // Assert
            changeHistoryLog.Id.ShouldBe(0);
            changeHistoryLog.ObjectKey.ShouldBe(TestObjectKey);
            changeHistoryLog.ObjectItemIdUuid.ShouldBe(objectItemId);
            changeHistoryLog.ObjectItemIdInt.ShouldBeNull();
            changeHistoryLog.FieldName.ShouldBe(fieldName);
            changeHistoryLog.OldValue.ShouldBe(oldValue);
            changeHistoryLog.NewValue.ShouldBe(newValue);
            changeHistoryLog.CreatedAt.ShouldNotBe(default(DateTime));
            (DateTime.UtcNow - changeHistoryLog.CreatedAt).TotalSeconds.ShouldBeLessThan(5);
            changeHistoryLog.CreatedBy.ShouldBe(createdBy);
        }

        [Fact]
        public void ParameterizedConstructor_WithNullValues_ShouldInitializeProperties()
        {
            // Arrange
            var objectItemId = 1002;
            var fieldName = "Email";
            string? oldValue = null;
            string? newValue = null;
            var createdBy = TestHelpers.TestUserId2;

            // Act
            var changeHistoryLog = new ChangeHistoryLog(
                TestObjectKey,
                objectItemId,
                fieldName,
                oldValue,
                newValue,
                createdBy);

            // Assert
            changeHistoryLog.Id.ShouldBe(0);
            changeHistoryLog.ObjectKey.ShouldBe(TestObjectKey);
            changeHistoryLog.ObjectItemIdInt.ShouldBe(objectItemId);
            changeHistoryLog.FieldName.ShouldBe(fieldName);
            changeHistoryLog.OldValue.ShouldBeNull();
            changeHistoryLog.NewValue.ShouldBeNull();
            changeHistoryLog.CreatedAt.ShouldNotBe(default(DateTime));
            changeHistoryLog.CreatedBy.ShouldBe(createdBy);
        }
        
        [Fact]
        public void ParameterizedConstructor_ShouldSetCreatedAtToUtcNow()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;
            
            // Act
            var changeHistoryLog = new ChangeHistoryLog(
                TestObjectKey,
                1003,
                "Email",
                "old@example.com",
                "new@example.com",
                TestHelpers.TestUserId3);
            
            var afterCreation = DateTime.UtcNow;
            
            // Assert
            changeHistoryLog.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreation);
            changeHistoryLog.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreation);
        }
    }
}
