using OXDesk.Core.Common.DTOs;
using OXDesk.Core.ValueLists.DTOs;

namespace OXDesk.Core.Identity.DTOs;

/// <summary>
/// Related reference data for a single user view/edit.
/// </summary>
public class UserDetailsRelated
{
    /// <summary>
    /// All countries as options.
    /// </summary>
    public IEnumerable<ValueListItemOption> Countries { get; set; } = Array.Empty<ValueListItemOption>();

    /// <summary>
    /// All user profiles as options.
    /// </summary>
    public IEnumerable<ValueListItemOption> Profiles { get; set; } = Array.Empty<ValueListItemOption>();

    /// <summary>
    /// All time zones as options (Name = display, Value = time zone ID).
    /// </summary>
    public IEnumerable<StringOption> TimeZones { get; set; } = Array.Empty<StringOption>();

    /// <summary>
    /// Status options: Active/true, Inactive/false.
    /// </summary>
    public IEnumerable<StatusOption> Statuses { get; set; } = Array.Empty<StatusOption>();

    /// <summary>
    /// The user's signature, if one exists.
    /// </summary>
    public UserSignatureResponse? Signature { get; set; }
}
