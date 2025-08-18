using System;

namespace OXDesk.Core.Identity.DTOs
{
    public sealed class CreatePermissionRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class UpdatePermissionRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class PermissionResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Display text for the last updater user, typically the user's full name.
        public string? UpdatedByText { get; set; }
    }

    public sealed class PermissionDetailsResponse
    {
        public PermissionResponse Data { get; set; } = new();
        public PermissionDetailsRelated Related { get; set; } = new();
    }

    public sealed class PermissionDetailsRelated
    {
        // Roles that have this permission (role names)
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
