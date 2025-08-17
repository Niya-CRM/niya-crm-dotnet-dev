using System.Collections.Generic;
using System.Linq;

namespace OXDesk.Core.Common.DTOs
{
    /// <summary>
    /// Generic paged response wrapper.
    /// </summary>
    public sealed class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public int PageNumber { get; set; }
        public int RowCount { get; set; }
    }
}
