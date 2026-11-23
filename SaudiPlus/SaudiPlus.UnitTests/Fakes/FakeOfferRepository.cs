using SaudiPlus.Domain.Entities;
using SaudiPlus.Application.Interfaces;

namespace SaudiPlus.UnitTests.Fakes
{
    public class FakeOfferRepository : IOfferRepository
    {
        public Offer? SavedOffer { get; private set; }
        public Task AddAsync(
            Offer offer, 
            CancellationToken cancellationToken = default
        )
        {
            SavedOffer = offer;
            return Task.CompletedTask;
        }

        public Task<Offer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var offer = SavedOffer?.Id == id
                ? SavedOffer
                : null;

            return Task.FromResult(offer);
        }
    }
}
