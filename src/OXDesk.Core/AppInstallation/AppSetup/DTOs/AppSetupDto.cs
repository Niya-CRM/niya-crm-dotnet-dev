using System.ComponentModel.DataAnnotations;

namespace OXDesk.Core.AppInstallation.AppSetup.DTOs;

/// <summary>
/// Data Transfer Object for the initial application installation/setup.
/// Contains information needed to create the first tenant and administrative user.
/// </summary>
public class AppSetupDto
{
    /// <summary>
    /// Gets or sets the tenant name.
    /// </summary>
    [Required(ErrorMessage = "Tenant name is required")]
    [StringLength(100, ErrorMessage = "Tenant name cannot exceed 100 characters")]
    public string TenantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host name.
    /// </summary>
    [Required(ErrorMessage = "Host is required")]
    [StringLength(100, ErrorMessage = "Host cannot exceed 100 characters")]
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the admin first name.
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the admin last name.
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the admin email.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the admin password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the admin password confirmation.
    /// </summary>
    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time zone.
    /// </summary>
    [Required(ErrorMessage = "Time zone is required")]
    public string TimeZone { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    [Required(ErrorMessage = "Location is required")]
    [StringLength(60, ErrorMessage = "Location cannot exceed 60 characters")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country code.
    /// </summary>
    [Required(ErrorMessage = "Country is required")]
    [StringLength(2, ErrorMessage = "Country code must be 2 characters")]
    public string? CountryCode { get; set; }
}
