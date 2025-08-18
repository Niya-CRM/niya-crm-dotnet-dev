using System.Collections.Generic;
using System.Linq;

namespace OXDesk.Core.Common.DTOs
{
    /// <summary>
    /// Generic list response wrapper with pagination metadata and related collection.
    /// Matches the requested pattern: data, pageNumber, rowCount, related.
    /// </summary>
    public sealed class PagedListWithRelatedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int PageNumber { get; set; }
        public int RowCount { get; set; }
        public IEnumerable<object> Related { get; set; } = Enumerable.Empty<object>();
    }
}
