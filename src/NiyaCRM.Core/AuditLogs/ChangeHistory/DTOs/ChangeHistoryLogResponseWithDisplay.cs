using NiyaCRM.Core.Common;
using System;

namespace NiyaCRM.Core.AuditLogs.ChangeHistory.DTOs
{
    /// <summary>
    /// DTO for change history log response with display values.
    /// </summary>
    public class ChangeHistoryLogResponseWithDisplay
    {
        /// <summary>
        /// Gets or sets the unique identifier of the change history log.
        /// </summary>
        public ValueDisplayPair<Guid> Id { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the object key (entity type) that was changed.
        /// </summary>
        public ValueDisplayPair<string> ObjectKey { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the object item ID (entity ID) that was changed.
        /// </summary>
        public ValueDisplayPair<Guid> ObjectItemId { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the name of the field that was changed.
        /// </summary>
        public ValueDisplayPair<string> FieldName { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the old value of the field before the change.
        /// </summary>
        public ValueDisplayPair<string> OldValue { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the new value of the field after the change.
        /// </summary>
        public ValueDisplayPair<string> NewValue { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the date and time when the change was made.
        /// </summary>
        public ValueDisplayPair<DateTime> CreatedAt { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the ID of the user who made the change.
        /// </summary>
        public ValueDisplayPair<Guid> CreatedBy { get; set; } = new();
    }
}
