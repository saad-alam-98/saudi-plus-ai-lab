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
    }
}
