using SaudiPlus.Application.Common.Models;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Application.Offers.GetOffers;
using SaudiPlus.Domain.Entities;
using System.Collections.Concurrent;

namespace SaudiPlus.Infrastructure.Repositories
{
    public class InMemoryOfferRepository : IOfferRepository
    {
        private static readonly ConcurrentDictionary<Guid, Offer> Offers = new();

        public Task AddAsync(Offer offer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Offers.TryAdd(offer.Id, offer))
                throw new InvalidOperationException(
                    "An offer with this ID already exists."
                );

            return Task.CompletedTask;
        }

        public Task<Offer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Offers.TryGetValue(id, out var offer);

            return Task.FromResult(offer);
        }

        public Task<PaginatedResult<Offer>> GetAllAsync(
            GetOffersQuery query,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<Offer> offers = Offers.Values;

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                offers = offers.Where(offer =>
                    offer.Title.Contains(
                        query.Search,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (query.MinDiscount.HasValue)
            {
                offers = offers.Where(offer =>
                    offer.DiscountPercentage >=
                    query.MinDiscount.Value);
            }

            var descending = string.Equals(
                query.SortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

            offers = query.SortBy.ToLowerInvariant() switch
            {
                "discount" => descending
                    ? offers.OrderByDescending(o => o.DiscountPercentage)
                        .ThenBy(o => o.Id)
                    : offers.OrderBy(o => o.DiscountPercentage)
                        .ThenBy(o => o.Id),

                "title" => descending
                    ? offers.OrderByDescending(o => o.Title)
                        .ThenBy(o => o.Id)
                    : offers.OrderBy(o => o.Title)
                        .ThenBy(o => o.Id),

                _ => descending
                    ? offers.OrderByDescending(o => o.StartDate)
                        .ThenBy(o => o.Id)
                    : offers.OrderBy(o => o.StartDate)
                        .ThenBy(o => o.Id)
            };

            var totalCount = offers.Count();

            var items = offers
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var result = new PaginatedResult<Offer>(
                items,
                query.Page,
                query.PageSize,
                totalCount
            );

            return Task.FromResult(result);
        }
    }
}
