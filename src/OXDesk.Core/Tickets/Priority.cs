using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Tickets;

[Table("priorities")]
public class Priority : AuditedEntityWithSoftDelete, IEntity, ITenantScoped
{
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public Guid TenantId { get; set; }

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string PriorityName { get; set; } = string.Empty;

    public int? IncrementScore { get; set; }

    // Audit fields inherited from AuditedEntityWithSoftDelete:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
}
