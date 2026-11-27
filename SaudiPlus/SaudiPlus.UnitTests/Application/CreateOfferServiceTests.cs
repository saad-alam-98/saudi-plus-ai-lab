using SaudiPlus.Application.Offers.CreateOffer;
using SaudiPlus.UnitTests.Fakes;

namespace SaudiPlus.UnitTests.Application
{
    public class CreateOfferServiceTests
    {
        private sealed class TrustedUser : SaudiPlus.Application.Interfaces.ICurrentUser
        {
            public Guid? UserId => Guid.NewGuid();
            public bool IsInRole(string role) => role == "Administrator";
        }
        private sealed class TrustedAccess : SaudiPlus.Application.Interfaces.IMerchantAccess
        {
            public Task<bool> ExistsAsync(Guid id, CancellationToken token = default) => Task.FromResult(true);
            public Task<bool> IsMemberAsync(Guid user, Guid id, CancellationToken token = default) => Task.FromResult(false);
        }
        [Fact]
        public async Task ExecuteAsync_ShouldSaveOffer_AndReturnId()
        {
            var repository = new FakeOfferRepository();
            var service = new CreateOfferService(repository, new TrustedUser(), new TrustedAccess());

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
