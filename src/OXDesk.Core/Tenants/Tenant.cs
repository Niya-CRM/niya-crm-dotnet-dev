using OXDesk.Core.Entities;

namespace OXDesk.Core.Tenants;

/// <summary>
/// Represents a tenant in the multi-tenant CRM system.
/// Pure domain entity with minimal business logic.
/// NOTE: Tenant itself is not ITenantScoped as it represents the tenant entity itself.
/// </summary>
public class Tenant : AuditedEntityWithSoftDelete, IEntityGuid
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
    /// Gets or sets the default time zone identifier for this tenant.
    /// </summary>
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;

    /// <summary>
    /// Gets or sets the user identifier associated with this tenant.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the database name for this tenant.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the database schema for this tenant (used in cloud hosting model).
    /// </summary>
    public string? Schema { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tenant is active.
    /// </summary>
    public string IsActive { get; set; } = "Y";

    // Audit fields inherited from AuditedEntityWithSoftDelete:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy

    /// <summary>
    /// Initializes a new instance of the <see cref="Tenant"/> class.
    /// Default constructor for Entity Framework and general use.
    /// </summary>
    public Tenant() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tenant"/> class without specifying the primary key.
    /// Id will be generated using <see cref="Guid.CreateVersion7"/>.
    /// </summary>
    /// <param name="name">The tenant name.</param>
    /// <param name="host">The tenant host.</param>
    /// <param name="email">The tenant email.</param>
    /// <param name="userId">The owner user identifier.</param>
    /// <param name="timeZone">The default time zone.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="isActive">Whether active.</param>
    /// <param name="createdAt">Creation timestamp.</param>
    /// <param name="createdBy">Creator user id.</param>
    public Tenant(string name, string host, string email, int userId, string timeZone, string? databaseName, string isActive, DateTime createdAt, int createdBy)
    {
        Id = Guid.CreateVersion7();
        Name = name;
        Host = host;
        Email = email;
        UserId = userId;
        TimeZone = timeZone;
        DatabaseName = databaseName;
        IsActive = isActive;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        UpdatedAt = createdAt;
        UpdatedBy = createdBy;
        DeletedAt = null;
    }
}
