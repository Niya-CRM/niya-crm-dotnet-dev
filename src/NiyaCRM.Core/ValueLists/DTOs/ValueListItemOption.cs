using System;

namespace NiyaCRM.Core.ValueLists.DTOs;

/// <summary>
/// Standard option DTO for value list items.
/// </summary>
public class ValueListItemOption
{
    public Guid Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemKey { get; set; } = string.Empty;
}
