using System.ComponentModel.DataAnnotations;
using NiyaCRM.Core.ValueLists.DTOs;
using NiyaCRM.Core.Common.DTOs;

namespace NiyaCRM.Core.Identity.DTOs;

/// <summary>
/// Request DTO for creating a new user.
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    [Required]
    [StringLength(256)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    [StringLength(30)]
    public string? FirstName { get; set; }
    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    [StringLength(30)]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the location of the user.
    /// </summary>
    [Required]
    [StringLength(60)]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password of the user.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time zone of the user.
    /// </summary>
    [StringLength(50)]
    public string? TimeZone { get; set; }
    
    /// <summary>
    /// Gets or sets the country code of the user.
    /// </summary>
    [StringLength(2)]
    public string? CountryCode { get; set; }

    /// <summary>
    /// Gets or sets the profile key (value list item key) associated with the user.
    /// </summary>
    public string? Profile { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Response DTO for user information.
/// </summary>
public class UserResponse
{
    /// <summary>
    /// Gets or sets the ID of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the location of the user.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time zone of the user.
    /// </summary>
    public string TimeZone { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the country code of the user.
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Gets or sets the display text for the country code, resolved from value list cache.
    /// </summary>
    public string? CountryCodeText { get; set; }

    /// <summary>
    /// Gets or sets the profile key (value list item key) associated with the user.
    /// </summary>
    public string? Profile { get; set; }

    /// <summary>
    /// Gets or sets the display text for the profile, resolved from value list cache.
    /// </summary>
    public string? ProfileText { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date when the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created the user.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last updated the user.
    /// </summary>
    public Guid UpdatedBy { get; set; }
}

/// <summary>
/// Wrapper response for users list with related reference data.
/// </summary>
public class UsersListResponse
{
    /// <summary>
    /// The user items.
    /// </summary>
    public IEnumerable<UserResponse> Data { get; set; } = Array.Empty<UserResponse>();

    /// <summary>
    /// Related reference data used to render filters/dropdowns.
    /// </summary>
    public UsersListRelated Related { get; set; } = new();
}

/// <summary>
/// Related reference data for the users list.
/// </summary>
public class UsersListRelated
{
    /// <summary>
    /// All user profiles as options.
    /// </summary>
    public IEnumerable<ValueListItemOption> Profiles { get; set; } = Array.Empty<ValueListItemOption>();

    /// <summary>
    /// Status options: Active/true, Inactive/false.
    /// </summary>
    public IEnumerable<StatusOption> Statuses { get; set; } = Array.Empty<StatusOption>();
}
