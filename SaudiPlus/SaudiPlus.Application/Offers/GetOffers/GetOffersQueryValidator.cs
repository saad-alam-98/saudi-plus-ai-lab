using SaudiPlus.Application.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaudiPlus.Application.Offers.GetOffers
{
    public static class GetOffersQueryValidator
    {
        public static void Validate(GetOffersQuery query)
        {
            if (query.Page < 1)
            {
                throw new ApplicationValidationException(
                    "Page must be greater than or equal to 1.");
            }

            if (query.PageSize < 1 || query.PageSize > 100)
            {
                throw new ApplicationValidationException(
                    "Page size must be between 1 and 100.");
            }

            if (query.Search is { Length: > 100 })
            {
                throw new ApplicationValidationException(
                    "Search must not exceed 100 characters.");
            }

            if (query.MinDiscount is < 0 or > 100)
            {
                throw new ApplicationValidationException(
                    "Minimum discount must be between 0 and 100.");
            }

            if (query.SortBy?.ToLowerInvariant()
                is not ("discount" or "title" or "startdate"))
            {
                throw new ApplicationValidationException(
                    "Unsupported sorting field.");
            }

            if (query.SortDirection?.ToLowerInvariant()
                is not ("asc" or "desc"))
            {
                throw new ApplicationValidationException(
                    "Sort direction must be asc or desc.");
            }

            var offset = ((long)query.Page - 1) * query.PageSize;

            if (offset > int.MaxValue)
            {
                throw new ApplicationValidationException(
                    "Requested page exceeds the supported range.");
            }
        }
    }
}
