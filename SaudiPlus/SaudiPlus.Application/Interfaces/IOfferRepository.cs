using SaudiPlus.Application.Common.Models;
using SaudiPlus.Application.Offers.GetOffers;
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

        Task<PaginatedResult<Offer>> GetAllAsync(
            GetOffersQuery query,
            CancellationToken cancellationToken = default
        );
    }
}
