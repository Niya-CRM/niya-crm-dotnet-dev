using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Identity;

/// <summary>
/// Represents a user's email signature containing contact and company information.
/// </summary>
public class UserSignature : ICreationAudited, IUpdationAudited, IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the user signature.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier this signature belongs to.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the complimentary close (e.g., "Kind regards", "Best wishes").
    /// </summary>
    [MaxLength(100)]
    public string? ComplimentaryClose { get; set; }

    /// <summary>
    /// Gets or sets the full name displayed in the signature.
    /// </summary>
    [MaxLength(100)]
    public string? FullName { get; set; }

    /// <summary>
    /// Gets or sets the job title displayed in the signature.
    /// </summary>
    [MaxLength(100)]
    public string? JobTitle { get; set; }

    /// <summary>
    /// Gets or sets the company name displayed in the signature.
    /// </summary>
    [MaxLength(200)]
    public string? Company { get; set; }

    /// <summary>
    /// Gets or sets the department displayed in the signature.
    /// </summary>
    [MaxLength(100)]
    public string? Department { get; set; }

    /// <summary>
    /// Gets or sets the first line of the address displayed in the signature.
    /// </summary>
    [MaxLength(200)]
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Gets or sets the second line of the address displayed in the signature.
    /// </summary>
    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Gets or sets the third line of the address displayed in the signature.
    /// </summary>
    [MaxLength(200)]
    public string? AddressLine3 { get; set; }

    /// <summary>
    /// Gets or sets the telephone number displayed in the signature.
    /// </summary>
    [MaxLength(30)]
    public string? Telephone { get; set; }

    /// <summary>
    /// Gets or sets the mobile number displayed in the signature.
    /// </summary>
    [MaxLength(30)]
    public string? Mobile { get; set; }

    /// <summary>
    /// Gets or sets the email address displayed in the signature.
    /// </summary>
    [MaxLength(255)]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the website URL displayed in the signature.
    /// </summary>
    [MaxLength(255)]
    public string? Website { get; set; }

    /// <summary>
    /// Gets or sets the free-style HTML signature content.
    /// Used when the global signature setting type is "free-style".
    /// </summary>
    [MaxLength(30000)]
    public string? FreeStyleSignature { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created this signature.
    /// </summary>
    [Required]
    public int CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this signature was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the ID of the user who last updated this signature.
    /// </summary>
    [Required]
    public int UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this signature was last updated.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the user this signature belongs to.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }
}
