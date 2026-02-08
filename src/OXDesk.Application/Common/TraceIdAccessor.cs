using System.Diagnostics;
using OXDesk.Core.Common;

namespace OXDesk.Application.Common;

/// <summary>
/// Implementation of ITraceIdAccessor that retrieves the trace ID from the current Activity.
/// </summary>
public class TraceIdAccessor : ITraceIdAccessor
{
    /// <inheritdoc/>
    public string? GetTraceId()
    {
        return Activity.Current?.Id;
    }
}
