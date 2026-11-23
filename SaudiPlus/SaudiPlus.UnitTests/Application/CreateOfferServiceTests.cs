using SaudiPlus.Application.Offers.CreateOffer;
using SaudiPlus.UnitTests.Fakes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaudiPlus.UnitTests.Application
{
    public class CreateOfferServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldSaveOffer_AndReturnId()
        {
            var repository = new FakeOfferRepository();
            var service = new CreateOfferService(repository);

            var request = new CreateOfferRequest(
                Guid.NewGuid(),
                "20% Discount",
                20,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(30)
            );

            var offerId = await service.ExecuteAsync(request);

            Assert.NotEqual(Guid.Empty, offerId);
            Assert.NotNull(repository.SavedOffer);
            Assert.Equal(offerId, repository.SavedOffer.Id);
            Assert.Equal(request.Title, repository.SavedOffer.Title);
        }
    }
}
