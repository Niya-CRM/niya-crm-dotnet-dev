using System;
using OXDesk.Core.AuditLogs.ChangeHistory;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Core.ChangeHistory
{
    public class ChangeHistoryLogTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializeProperties()
        {
            // Act
            var changeHistoryLog = new ChangeHistoryLog();

            // Assert
            changeHistoryLog.Id.ShouldBe(0);
            changeHistoryLog.ObjectKey.ShouldBe(string.Empty);
            changeHistoryLog.ObjectItemId.ShouldBe(0);
            changeHistoryLog.FieldName.ShouldBe(string.Empty);
            changeHistoryLog.OldValue.ShouldBeNull();
            changeHistoryLog.NewValue.ShouldBeNull();
            changeHistoryLog.CreatedAt.ShouldBe(default(DateTime));
            changeHistoryLog.CreatedBy.ShouldBe(0);
        }

        [Fact]
        public void ParameterizedConstructor_ShouldInitializeProperties()
        {
            // Arrange
            var objectKey = "User";
            var objectItemId = 1001;
            var fieldName = "Email";
            var oldValue = "old@example.com";
            var newValue = "new@example.com";
            var createdBy = 10001;

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
            changeHistoryLog.ObjectItemId.ShouldBe(objectItemId);
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
            var objectItemId = 1002;
            var fieldName = "Email";
            string? oldValue = null;
            string? newValue = null;
            var createdBy = 10002;

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
            changeHistoryLog.ObjectItemId.ShouldBe(objectItemId);
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
                1003,
                "Email",
                "old@example.com",
                "new@example.com",
                10003);
            
            var afterCreation = DateTime.UtcNow;
            
            // Assert
            changeHistoryLog.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreation);
            changeHistoryLog.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreation);
        }
    }
}
