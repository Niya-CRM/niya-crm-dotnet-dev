using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace OXDesk.Core.Identity;

/// <summary>
/// Custom application user login that uses Guid as primary key with tenant_id.
/// </summary>
public class ApplicationUserLogin : IdentityUserLogin<int>
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [System.ComponentModel.DataAnnotations.Required]
    public int TenantId { get; set; }
}
