using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Tickets;

/// <summary>
/// Represents a holiday associated with a business hours schedule.
/// </summary>
[Table("holidays")]
public class Holiday : AuditedEntityWithSoftDelete, IEntity
{
    /// <inheritdoc />
    [Key]
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the associated business hours schedule identifier.
    /// </summary>
    [Required]
    public int BusinessHourId { get; set; }

    /// <summary>
    /// Gets or sets the name of the holiday.
    /// </summary>
    [Required]
    [StringLength(60)]
    [Column(TypeName = "varchar(60)")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of the holiday (date only, no time component).
    /// </summary>
    [Required]
    public DateOnly Date { get; set; }
}
