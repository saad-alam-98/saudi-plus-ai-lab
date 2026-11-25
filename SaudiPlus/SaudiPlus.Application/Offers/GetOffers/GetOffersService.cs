using SaudiPlus.Application.Interfaces;
using SaudiPlus.Application.Common.Models;
using SaudiPlus.Application.Offers.GetOffer;

namespace SaudiPlus.Application.Offers.GetOffers
{
    public class GetOffersService
    {
        private readonly IOfferRepository _repository;

        public GetOffersService(IOfferRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedResult<OfferResponse>> ExecuteAsync(
            GetOffersQuery query,
            CancellationToken cancellationToken = default
        )
        {
            GetOffersQueryValidator.Validate(query);

            var result = await _repository.GetAllAsync(
                query,
                cancellationToken
            );

            var items = result.Items.Select(offer =>
                new OfferResponse(
                    offer.Id,
                    offer.Title,
                    offer.DiscountPercentage
                )).ToList();

            return new PaginatedResult<OfferResponse>(
                items,
                result.Page,
                result.PageSize,
                result.TotalCount
            );
        }
    }
}
