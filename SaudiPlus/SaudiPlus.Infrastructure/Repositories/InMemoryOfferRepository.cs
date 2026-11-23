using SaudiPlus.Domain.Entities;
using System.Collections.Concurrent;
using SaudiPlus.Application.Interfaces;

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
    }
}
