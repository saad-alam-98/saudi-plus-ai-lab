using SaudiPlus.Application.Offers.GetOffer;
using SaudiPlus.Domain.Entities;
using SaudiPlus.UnitTests.Fakes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaudiPlus.UnitTests.Application
{
    public class GetOfferServiceTests
    {
        [Fact]
        public async Task ExecuteAsync_ShouldReturnOffer_WhenOfferExists()
        {
            var repository = new FakeOfferRepository();
            var service = new GetOfferService(repository);

            var startDate = DateTime.UtcNow;

            var offer = new Offer(
                Guid.NewGuid(),
                "20% Discount",
                20,
                startDate,
                startDate.AddDays(30)
            );

            await repository.AddAsync(offer);

            var result = await service.ExecuteAsync(offer.Id);

            Assert.NotNull(result);
            Assert.Equal(offer.Id, result.Id);
            Assert.Equal(offer.Title, result.Title);
            Assert.Equal(
                offer.DiscountPercentage,
                result.DiscountPercentage);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnNull_WhenOfferDoesNotExist()
        {
            // Arrange
            var repository = new FakeOfferRepository();
            var service = new GetOfferService(repository);

            // Act
            var result = await service.ExecuteAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }
    }
}
