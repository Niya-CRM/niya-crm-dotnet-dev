using System;
using Microsoft.AspNetCore.Identity;

namespace NiyaCRM.Core.Identity;

/// <summary>
/// Custom application role that uses Guid as primary key.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
