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
    }
}
