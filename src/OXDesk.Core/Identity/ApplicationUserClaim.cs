using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Identity;

/// <summary>
/// Custom application user claim that uses int as primary key with tenant_id.
/// </summary>
public class ApplicationUserClaim : IdentityUserClaim<int>
{
}
