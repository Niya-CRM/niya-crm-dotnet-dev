namespace OXDesk.Core.Common;

/// <summary>
/// Provides access to the current correlation ID for the request.
/// </summary>
public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Gets the correlation ID for the current request.
    /// </summary>
    /// <returns>The correlation ID, or null if not available.</returns>
    string? GetCorrelationId();
}
