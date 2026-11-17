using SaudiPlus.Domain.Entities;
using SaudiPlus.Application.Interfaces;

namespace SaudiPlus.Application.Offers.CreateOffer
{
    public class CreateOfferService
    {
        private readonly IOfferRepository _repository;

        public CreateOfferService(IOfferRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> ExecuteAsync(
            CreateOfferRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var offer = new Offer(
                request.MerchantId,
                request.Title,
                request.DiscountPercentage,
                request.StartDate,
                request.EndDate
            );

            await _repository.AddAsync(offer, cancellationToken);
            return offer.Id;
        }
    }
}
