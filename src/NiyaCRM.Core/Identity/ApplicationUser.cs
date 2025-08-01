using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Core.Identity
{
    /// <summary>
    /// Application user entity that extends IdentityUser.
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [MaxLength(30)]
        public string? FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [MaxLength(30)]
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the default time zone identifier for this user.
        /// </summary>
        public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
        
        /// <summary>
        /// Gets or sets the date when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the user who created the user.
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date when the user was deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Gets or sets the date when the user was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the user who last updated the user.
        /// </summary>
        public Guid UpdatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user is active.
        /// </summary>
        [MaxLength(1)]
        public string IsActive { get; set; } = "Y";
    }
}
