using SaudiPlus.Domain.Entities;

namespace SaudiPlus.UnitTests.Domain
{
    public class OfferTests
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenDiscountExceeds100()
        {
            var merchantId = Guid.NewGuid();
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(30);

            Action action = () => new Offer(
                merchantId,
                "Test Offer",
                150,
                startDate,
                endDate
            );

            Assert.Throws<ArgumentException>(action);
        }
    }
}
