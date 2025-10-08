using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Tickets;

[Table("brands")]
public class Brand : AuditedEntityWithSoftDelete, IEntity, ITenantScoped
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
    public string BrandName { get; set; } = string.Empty;

    [StringLength(1000)]
    [Column(TypeName = "varchar(1000)")]
    public string? Logo { get; set; }

    [StringLength(1000)]
    [Column(TypeName = "varchar(1000)")]
    public string? LogoDark { get; set; }

    [StringLength(10)]
    [Column(TypeName = "varchar(10)")]
    public string? BrandColor { get; set; }

    [StringLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string? Website { get; set; }

    // Audit fields inherited from AuditedEntityWithSoftDelete:
    // - CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy
}
