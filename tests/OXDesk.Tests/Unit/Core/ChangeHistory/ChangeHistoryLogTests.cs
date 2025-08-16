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
            changeHistoryLog.Id.ShouldBe(Guid.Empty);
            changeHistoryLog.ObjectKey.ShouldBe(string.Empty);
            changeHistoryLog.ObjectItemId.ShouldBe(Guid.Empty);
            changeHistoryLog.FieldName.ShouldBe(string.Empty);
            changeHistoryLog.OldValue.ShouldBeNull();
            changeHistoryLog.NewValue.ShouldBeNull();
            changeHistoryLog.CreatedAt.ShouldBe(default(DateTime));
            changeHistoryLog.CreatedBy.ShouldBe(Guid.Empty);
        }

        [Fact]
        public void ParameterizedConstructor_ShouldInitializeProperties()
        {
            // Arrange
            var id = Guid.CreateVersion7();
            var objectKey = "User";
            var objectItemId = Guid.CreateVersion7();
            var fieldName = "Email";
            var oldValue = "old@example.com";
            var newValue = "new@example.com";
            var createdBy = Guid.CreateVersion7();

            // Act
            var changeHistoryLog = new ChangeHistoryLog(
                id,
                objectKey,
                objectItemId,
                fieldName,
                oldValue,
                newValue,
                createdBy);

            // Assert
            changeHistoryLog.Id.ShouldBe(id);
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
            var id = Guid.CreateVersion7();
            var objectKey = "User";
            var objectItemId = Guid.CreateVersion7();
            var fieldName = "Email";
            string? oldValue = null;
            string? newValue = null;
            var createdBy = Guid.CreateVersion7();

            // Act
            var changeHistoryLog = new ChangeHistoryLog(
                id,
                objectKey,
                objectItemId,
                fieldName,
                oldValue,
                newValue,
                createdBy);

            // Assert
            changeHistoryLog.Id.ShouldBe(id);
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
                Guid.CreateVersion7(),
                "User",
                Guid.CreateVersion7(),
                "Email",
                "old@example.com",
                "new@example.com",
                Guid.CreateVersion7());
            
            var afterCreation = DateTime.UtcNow;
            
            // Assert
            changeHistoryLog.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreation);
            changeHistoryLog.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreation);
        }
    }
}
