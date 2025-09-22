using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Tickets;

[Table("channels")]
public class Channel
{
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    [Column("tenant_id")]
    [Required]
    public int TenantId { get; set; }

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string ChannelName { get; set; } = string.Empty;

    [Required]
    public int CreatedBy { get; set; }

    [Required]
    public int UpdatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
