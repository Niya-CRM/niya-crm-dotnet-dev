using System;

namespace OXDesk.Core.Common.DTOs
{
    /// <summary>
    /// Generic single-entity response wrapper with a related payload.
    /// Matches the pattern: data, related.
    /// </summary>
    /// <typeparam name="TData">Type of the data object.</typeparam>
    /// <typeparam name="TRelated">Type of the related object.</typeparam>
    public sealed class EntityWithRelatedResponse<TData, TRelated>
    {
        public TData Data { get; set; } = default!;
        public TRelated Related { get; set; } = default!;
    }
}
