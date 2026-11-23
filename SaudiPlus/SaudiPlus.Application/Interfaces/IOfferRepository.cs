using SaudiPlus.Domain.Entities;

namespace SaudiPlus.Application.Interfaces
{
    public interface IOfferRepository
    {
        Task AddAsync(
            Offer offer,
            CancellationToken cancellationToken = default
        );

        Task<Offer?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );
    }
}
