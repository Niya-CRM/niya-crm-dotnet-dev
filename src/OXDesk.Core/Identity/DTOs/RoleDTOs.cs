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
    }

    public sealed class RoleDetailsResponse
    {
        public RoleResponse Data { get; set; } = default!;
        public string[] Permissions { get; set; } = Array.Empty<string>();
    }

    public sealed class UpdateRolePermissionsRequest
    {
        public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
