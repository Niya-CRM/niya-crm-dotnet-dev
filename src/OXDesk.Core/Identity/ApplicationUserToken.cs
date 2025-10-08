using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Identity;

/// <summary>
/// Custom application user token that uses Guid as primary key with tenant_id.
/// </summary>
public class ApplicationUserToken : IdentityUserToken<Guid>, ITenantScoped
{
    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [System.ComponentModel.DataAnnotations.Required]
    public Guid TenantId { get; set; }
}
