using System;
using System.Text;

namespace SaudiPlus.Domain.Entities
{
    public class Offer
    {
        public Guid Id { get; private set; }
        public Guid MerchantId { get; private set; }
        public string Title { get; private set; }
        public decimal DiscountPercentage { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }

        public Offer(
            Guid merchantId,
            string title,
            decimal discountPercentage,
            DateTime startDate,
            DateTime endDate
        )
        {
            if (merchantId == Guid.Empty)
                throw new ArgumentException("Merchant is required.");

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Offer title is required.");

            if (discountPercentage <= 0 || discountPercentage > 100)
                throw new ArgumentException("Discount must be between 0 and 100.");

            if (endDate <= startDate)
                throw new ArgumentException("End date must be after start date.");

            Id = Guid.NewGuid();
            MerchantId = merchantId;
            Title = title;
            DiscountPercentage = discountPercentage;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
