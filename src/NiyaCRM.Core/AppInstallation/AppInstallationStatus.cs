using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NiyaCRM.Core.AppInstallation;

/// <summary>
/// Represents the status of application installation steps or pipeline processes.
/// </summary>
public class AppInstallationStatus
{
    /// <summary>
    /// Gets or sets the unique identifier for the installation status record.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the pipeline name or identifier. Eg: Initial, Upgrade
    /// </summary>
    [Required]
    [StringLength(15)]
    public string Pipeline { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the application. Eg: 1.0.0
    /// </summary>
    [Required]
    [StringLength(10)]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order of the step within the pipeline. Eg: 1, 2, 3
    /// </summary>
    [Required]
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the step name or description within the pipeline. Eg: Create Database, Create Tables, Create Users
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Step { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the step is completed.
    /// Y = Completed, N = Not Completed
    /// </summary>
    [Required]
    [StringLength(1)]
    public string Completed { get; set; } = "N";
}
