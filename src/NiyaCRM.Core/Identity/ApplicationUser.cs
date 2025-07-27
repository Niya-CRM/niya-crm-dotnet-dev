using System;
using Microsoft.AspNetCore.Identity;

namespace NiyaCRM.Core.Identity
{
    /// <summary>
    /// Application user entity that extends IdentityUser.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string? FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string? LastName { get; set; }
        
        /// <summary>
        /// Gets or sets the tenant ID that this user belongs to.
        /// </summary>
        public Guid? TenantId { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets a value indicating whether the user is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
