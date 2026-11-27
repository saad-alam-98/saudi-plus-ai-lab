using SaudiPlus.Domain.Entities;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Application.Common.Exceptions;

namespace SaudiPlus.Application.Offers.CreateOffer
{
    public class CreateOfferService
    {
        private readonly IOfferRepository _repository;
        private readonly ICurrentUser _currentUser;
        private readonly IMerchantAccess _merchantAccess;

        public CreateOfferService(IOfferRepository repository, ICurrentUser currentUser, IMerchantAccess merchantAccess)
        {
            _repository = repository;
            _currentUser = currentUser;
            _merchantAccess = merchantAccess;
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

            var userId = _currentUser.UserId ?? throw new AuthenticationRequiredException();
            var administrator = _currentUser.IsInRole("Administrator");
            if (!administrator && !_currentUser.IsInRole("Merchant"))
                throw new OfferForbiddenException();
            if (!await _merchantAccess.ExistsAsync(request.MerchantId, cancellationToken))
                throw new ApplicationValidationException("Merchant does not exist.");
            if (!administrator && !await _merchantAccess.IsMemberAsync(userId, request.MerchantId, cancellationToken))
                throw new OfferForbiddenException();

            await _repository.AddAsync(offer, cancellationToken);
            return offer.Id;
        }
    }
}
