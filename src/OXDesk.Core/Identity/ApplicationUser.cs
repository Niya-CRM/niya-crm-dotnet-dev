using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Identity
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
        /// Gets or sets the location of the user.
        /// </summary>
        [Required]
        [MaxLength(60)]
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the country code of the user.
        /// </summary>
        [MaxLength(2)]
        public string? CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the profile key (value list item key) associated with the user.
        /// </summary>
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? Profile { get; set; }

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
