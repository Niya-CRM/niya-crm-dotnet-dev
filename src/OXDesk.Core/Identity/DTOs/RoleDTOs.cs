using System;
using System.Collections.Generic;

namespace OXDesk.Core.Identity.DTOs
{
    public sealed class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class UpdateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public sealed class RoleResponse
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

    public sealed class RoleDetailsResponse
    {
        public RoleResponse Data { get; set; } = default!;
        public RoleDetailsRelated Related { get; set; } = new();
    }

    public sealed class RoleDetailsRelated
    {
        public string[] Permissions { get; set; } = Array.Empty<string>();
    }

    public sealed class UpdateRolePermissionsRequest
    {
        public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
