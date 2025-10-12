using System;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Tests.Helpers;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Core.ChangeHistory
{
    public class ChangeHistoryLogTests
    {
        [Fact]
        public void ParameterizedConstructor_ShouldInitializeProperties()
        {
            // Arrange
            var objectKey = "User";
            var objectItemId = Guid.Parse("00000000-0000-0000-0000-0000000003E9"); // 1001
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
            changeHistoryLog.Id.ShouldBe(Guid.Empty);
            changeHistoryLog.ObjectKey.ShouldBe(objectKey);
            changeHistoryLog.ObjectItemIdUuid.ShouldBe(objectItemId);
            changeHistoryLog.FieldName.ShouldBe(fieldName);
            changeHistoryLog.OldValue.ShouldBe(oldValue);
            changeHistoryLog.NewValue.ShouldBe(newValue);
            changeHistoryLog.CreatedAt.ShouldNotBe(default(DateTime));
            // CreatedAt should be within the last second (allowing for test execution time)
            (DateTime.UtcNow - changeHistoryLog.CreatedAt).TotalSeconds.ShouldBeLessThan(1);
            changeHistoryLog.CreatedBy.ShouldBe(createdBy);
        }

        [Fact]
        public void ParameterizedConstructor_WithNullValues_ShouldInitializeProperties()
        {
            // Arrange
            var objectKey = "User";
            var objectItemId = Guid.Parse("00000000-0000-0000-0000-0000000003EA"); // 1002
            var fieldName = "Email";
            string? oldValue = null;
            string? newValue = null;
            var createdBy = TestHelpers.TestUserId2;

            // Act
            var changeHistoryLog = new ChangeHistoryLog(
                objectKey,
                objectItemId,
                fieldName,
                oldValue,
                newValue,
                createdBy);

            // Assert
            changeHistoryLog.Id.ShouldBe(Guid.Empty);
            changeHistoryLog.ObjectKey.ShouldBe(objectKey);
            changeHistoryLog.ObjectItemIdUuid.ShouldBe(objectItemId);
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
                "User",
                Guid.Parse("00000000-0000-0000-0000-0000000003EB"), // 1003
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
