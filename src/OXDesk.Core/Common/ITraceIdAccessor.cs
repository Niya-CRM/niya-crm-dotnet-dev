namespace OXDesk.Core.Common;

/// <summary>
/// Provides access to the current trace ID for the request.
/// </summary>
public interface ITraceIdAccessor
{
    /// <summary>
    /// Gets the trace ID for the current request.
    /// </summary>
    /// <returns>The trace ID, or null if not available.</returns>
    string? GetTraceId();
}
