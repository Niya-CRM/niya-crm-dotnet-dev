namespace NiyaCRM.Core.Onboarding.DTOs;

/// <summary>
/// Data Transfer Object for the initial application installation/onboarding.
/// Contains information needed to create the first tenant and administrative user.
/// </summary>
public class AppInstallationDto
{
    /// <summary>
    /// Gets or sets the name of the organization/tenant.
    /// </summary>
    public string TenantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host/domain for the tenant.
    /// </summary>
    public string TenantHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary email address for the tenant.
    /// </summary>
    public string TenantEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name of the admin user.
    /// </summary>
    public string AdminFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the admin user.
    /// </summary>
    public string AdminLastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the admin user.
    /// </summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for the admin user.
    /// </summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timezone for the tenant.
    /// </summary>
    public string TimeZone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the preferred language/locale for the tenant.
    /// </summary>
    public string Locale { get; set; } = "en-US";
}
