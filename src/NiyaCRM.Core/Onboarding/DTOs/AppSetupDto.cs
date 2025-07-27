using System.ComponentModel.DataAnnotations;

namespace NiyaCRM.Core.Onboarding.DTOs;

/// <summary>
/// Data submitted from the initial setup page to create the first tenant and admin user.
/// </summary>
public class AppSetupDto
{
    // Tenant
    [Required]
    [MaxLength(50)]
    public string TenantName { get; set; } = null!;
    [Required]
    [MaxLength(100)]
    public string Host { get; set; } = null!;

    // Admin user
    [Required]
    [MaxLength(20)]
    public string FirstName { get; set; } = null!;
    [Required]
    [MaxLength(20)]
    public string LastName { get; set; } = null!;
    [Required]
    [MaxLength(100)]
    public string AdminEmail { get; set; } = null!;
    [Required]
    [MaxLength(25)]
    public string Password { get; set; } = null!;
    [Required]
    [MaxLength(25)]
    public string ConfirmPassword { get; set; } = null!;

    // TimeZone
    [Required]
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
}
