
namespace OXDesk.Core.Common.Response;

public class FieldDto
{
    public string FieldKey { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldValue { get; set; } = string.Empty;
    public string? DisplayValue { get; set; }
}
