namespace NiyaCRM.Core.Tenants;

/// <summary>
/// Represents a tenant in the multi-tenant CRM system.
/// Pure domain entity with minimal business logic.
/// </summary>
public class Tenant
{
    /// <summary>
    /// Gets or sets the unique identifier for the tenant.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the tenant.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host/domain associated with this tenant.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address associated with this tenant.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database name for this tenant.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tenant is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the tenant was last modified.
    /// </summary>
    public DateTime LastModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the user who created the tenant.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who last modified the tenant.
    /// </summary>
    public string LastModifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tenant"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public Tenant() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tenant"/> class.
    /// Constructor for creating new tenant instances with all required properties.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="name">The tenant name.</param>
    /// <param name="host">The tenant host.</param>
    /// <param name="email">The tenant email.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="isActive">Whether the tenant is active.</param>
    /// <param name="createdAt">The creation timestamp.</param>
    /// <param name="createdBy">The user who created the tenant.</param>
    public Tenant(Guid id, string name, string host, string email, string? databaseName, bool isActive, DateTime createdAt, string createdBy)
    {
        Id = id;
        Name = name;
        Host = host;
        Email = email;
        DatabaseName = databaseName;
        IsActive = isActive;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        LastModifiedAt = createdAt;
        LastModifiedBy = createdBy;
    }
}
