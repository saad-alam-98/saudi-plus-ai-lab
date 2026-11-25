using System;
using System.Collections.Generic;
using System.Text;

namespace SaudiPlus.Application.Offers.GetOffers
{
    public sealed record GetOffersQuery(
        string? Search = null,
        decimal? MinDiscount = null,
        string SortBy = "startDate",
        string SortDirection = "desc",
        int Page = 1,
        int PageSize = 20
    );
}
