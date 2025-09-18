using System;

namespace OXDesk.Core.ValueLists.DTOs;

/// <summary>
/// Standard option DTO for value list items.
/// </summary>
public class ValueListItemOption
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemKey { get; set; } = string.Empty;
    public int? Order { get; set; }
    public bool IsActive { get; set; }
}
