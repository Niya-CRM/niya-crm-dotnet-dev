using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OXDesk.Core.Tickets;

[Table("request_types")]
public class RequestType
{
    [Key]
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string RequestTypeName { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Column(TypeName = "varchar(30)")]
    public string RequestTypeKey { get; set; } = string.Empty;

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
