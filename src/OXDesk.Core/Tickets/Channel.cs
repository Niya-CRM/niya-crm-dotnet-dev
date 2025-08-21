using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Tickets;

[Table("channels")]
public class Channel
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string ChannelName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string ChannelKey { get; set; } = string.Empty;

    [Required]
    public Guid CreatedBy { get; set; }

    [Required]
    public Guid UpdatedBy { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}
