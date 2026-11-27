using SaudiPlus.Application.Common.Exceptions;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Application.Offers.CreateOffer;
using SaudiPlus.UnitTests.Fakes;
namespace SaudiPlus.UnitTests.Application;
public class CreateOfferAuthorizationTests
{
    private sealed class User(Guid? id, params string[] roles) : ICurrentUser
    {
        public Guid? UserId => id;
        public bool IsInRole(string role) => roles.Contains(role);
    }
    private sealed class Access(bool exists, bool member) : IMerchantAccess
    {
        public int MembershipChecks { get; private set; }
        public Task<bool> ExistsAsync(Guid merchantId, CancellationToken token = default) => Task.FromResult(exists);
        public Task<bool> IsMemberAsync(Guid userId, Guid merchantId, CancellationToken token = default)
        { MembershipChecks++; return Task.FromResult(member); }
    }
    private static CreateOfferRequest Request() { var start = DateTime.UtcNow; return new(Guid.NewGuid(), "Offer", 20m, start, start.AddDays(1)); }
    [Fact]
    public async Task AnonymousCannotCreate()
    {
        var repo = new FakeOfferRepository();
        await Assert.ThrowsAsync<AuthenticationRequiredException>(() => new CreateOfferService(repo, new User(null), new Access(true, true)).ExecuteAsync(Request()));
        Assert.Null(repo.SavedOffer);
    }
    [Theory]
    [InlineData("Customer", true, true, false)]
    [InlineData("Merchant", true, false, false)]
    [InlineData("Merchant", true, true, true)]
    [InlineData("Administrator", true, false, true)]
    public async Task RoleAndOwnershipAreEnforced(string role, bool exists, bool member, bool allowed)
    {
        var repo = new FakeOfferRepository(); var access = new Access(exists, member);
        var service = new CreateOfferService(repo, new User(Guid.NewGuid(), role), access);
        if (allowed) Assert.NotEqual(Guid.Empty, await service.ExecuteAsync(Request()));
        else await Assert.ThrowsAsync<OfferForbiddenException>(() => service.ExecuteAsync(Request()));
        Assert.Equal(allowed, repo.SavedOffer is not null);
        if (role == "Administrator") Assert.Equal(0, access.MembershipChecks);
    }
    [Fact]
    public async Task UnknownMerchantCannotReceiveOffer()
    {
        var repo = new FakeOfferRepository();
        await Assert.ThrowsAsync<ApplicationValidationException>(() => new CreateOfferService(repo, new User(Guid.NewGuid(), "Administrator"), new Access(false, false)).ExecuteAsync(Request()));
        Assert.Null(repo.SavedOffer);
    }
}
