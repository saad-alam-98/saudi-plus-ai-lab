using System;
using System.Collections.Generic;
using System.Text;

namespace SaudiPlus.Application.Common.Models
{
    public sealed record PaginatedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount
    )
    {
        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasNextPage =>
            Page < TotalPages;

        public bool HasPreviousPage =>
            Page > 1;
    }
}
