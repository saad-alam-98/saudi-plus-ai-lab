using SaudiPlus.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaudiPlus.Application.Offers.GetOffer
{
    public class GetOfferService
    {
        private readonly IOfferRepository _repository;

        public GetOfferService(IOfferRepository repository)
        {
            _repository = repository;
        }

        public async Task<OfferResponse?> ExecuteAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            var offer = await _repository.GetByIdAsync(
                id, cancellationToken
            );

            if (offer is null)
                return null;

            return new OfferResponse(
                offer.Id,
                offer.Title,
                offer.DiscountPercentage
            );
        }
    }
}
