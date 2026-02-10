using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OXDesk.Core.Entities;

namespace OXDesk.Core.Settings;

/// <summary>
/// Represents a generic application setting stored as a key/value pair.
/// </summary>
[Table("settings")]
public class Setting : AuditedEntity, IEntity
{
    /// <inheritdoc />
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique setting key (e.g. "signature", "TicketMaxAttachments").
    /// </summary>
    [Required]
    [StringLength(255)]
    [Column(TypeName = "varchar(255)")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the setting value.
    /// </summary>
    [Column(TypeName = "text")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value type.
    /// Must be one of the values defined in <see cref="SettingConstant.ValueTypes"/>.
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string ValueType { get; set; } = SettingConstant.ValueTypes.String;
}
