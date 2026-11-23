using System;

namespace SaudiPlus.Application.Offers.GetOffer
{
    public sealed record OfferResponse(
        Guid Id,
        string Title,
        decimal DiscountPercentage,
        string MerchantName
    );
}

