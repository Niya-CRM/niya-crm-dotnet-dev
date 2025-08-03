using NiyaCRM.Core.Common;

namespace NiyaCRM.Core.Identity.DTOs;

/// <summary>
/// Enhanced response DTO for user information with display values.
/// </summary>
public class UserResponseWithDisplay
{
    /// <summary>
    /// Gets or sets the ID of the user.
    /// </summary>
    public ValueDisplayPair<Guid> Id { get; set; } = new();

    /// <summary>
    /// Gets or sets the email of the user.
    /// </summary>
    public ValueDisplayPair<string> Email { get; set; } = new();

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    public ValueDisplayPair<string> UserName { get; set; } = new();

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public ValueDisplayPair<string> FirstName { get; set; } = new();

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public ValueDisplayPair<string> LastName { get; set; } = new();

    /// <summary>
    /// Gets or sets the time zone of the user.
    /// </summary>
    public ValueDisplayPair<string> TimeZone { get; set; } = new();

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// </summary>
    public ValueDisplayPair<string> PhoneNumber { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the user is active.
    /// </summary>
    public ValueDisplayPair<bool> IsActive { get; set; } = new();

    /// <summary>
    /// Gets or sets the date when the user was created.
    /// </summary>
    public ValueDisplayPair<DateTime> CreatedAt { get; set; } = new();

    /// <summary>
    /// Gets or sets the date when the user was last updated.
    /// </summary>
    public ValueDisplayPair<DateTime> UpdatedAt { get; set; } = new();

    /// <summary>
    /// Gets or sets the ID of the user who created the user.
    /// </summary>
    public ValueDisplayPair<Guid> CreatedBy { get; set; } = new();

    /// <summary>
    /// Gets or sets the ID of the user who last updated the user.
    /// </summary>
    public ValueDisplayPair<Guid> UpdatedBy { get; set; } = new();
}
