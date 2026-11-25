using SaudiPlus.Domain.Exceptions;

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
                throw new DomainValidationException("Merchant is required.");

            if (string.IsNullOrWhiteSpace(title))
                throw new DomainValidationException("Offer title is required.");

            if (title.Length > 200)
                throw new DomainValidationException("Offer title must not exceed 200 characters.");

            if (discountPercentage <= 0 || discountPercentage > 100)
                throw new DomainValidationException("Discount must be between 0 and 100.");

            if (discountPercentage != decimal.Truncate(discountPercentage * 100m) / 100m)
                throw new DomainValidationException("Discount must have at most two decimal places.");

            if (startDate.Kind != DateTimeKind.Utc || endDate.Kind != DateTimeKind.Utc)
                throw new DomainValidationException("Start and end dates must be UTC.");

            if (endDate <= startDate)
                throw new DomainValidationException("End date must be after start date.");

            Id = Guid.NewGuid();
            MerchantId = merchantId;
            Title = title;
            DiscountPercentage = discountPercentage;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
