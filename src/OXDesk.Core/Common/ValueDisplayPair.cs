namespace OXDesk.Core.Common;

/// <summary>
/// Field value with display value pair.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class ValueDisplayPair<T>
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public T? Value { get; set; }
    
    /// <summary>
    /// Gets or sets the display value.
    /// </summary>
    public string? DisplayValue { get; set; }
}
