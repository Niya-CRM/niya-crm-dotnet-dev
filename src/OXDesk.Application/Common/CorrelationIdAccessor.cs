using Microsoft.AspNetCore.Http;
using OXDesk.Core.Common;

namespace OXDesk.Application.Common;

/// <summary>
/// Implementation of ICorrelationIdAccessor that retrieves the correlation ID from HttpContext.
/// </summary>
public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdAccessor"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public CorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public string? GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue(CorrelationIdHeader, out var correlationId) == true)
        {
            return correlationId?.ToString();
        }
        return null;
    }
}
