using System;
using System.Text;

namespace SaudiPlus.Application.Offers.CreateOffer
{
    public sealed record CreateOfferRequest(
        Guid MerchantId,
        string Title,
        decimal DiscountPercentage,
        DateTime StartDate,
        DateTime EndDate
    );
}
