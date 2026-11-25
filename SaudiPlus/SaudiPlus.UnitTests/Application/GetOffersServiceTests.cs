using SaudiPlus.Application.Common.Exceptions;
using SaudiPlus.Application.Offers.GetOffers;
using SaudiPlus.Domain.Entities;
using SaudiPlus.UnitTests.Fakes;

namespace SaudiPlus.UnitTests.Application;

public class GetOffersServiceTests
{
    [Fact]
    public async Task ValidQueryIsForwardedAndEntitiesMapped()
    {
        var repository = new FakeOfferRepository();
        var offers = Enumerable.Range(1, 3).Select(i => new Offer(Guid.NewGuid(), $"Offer {i}", i * 10m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1))).ToArray();
        foreach (var offer in offers) await repository.AddAsync(offer);
        var query = new GetOffersQuery(Search: "Offer", MinDiscount: 10m, SortBy: "title", SortDirection: "asc", Page: 2, PageSize: 2);

        var result = await new GetOffersService(repository).ExecuteAsync(query);

        Assert.Equal(1, repository.GetAllAsyncCallCount);
        Assert.Equal(query, repository.ReceivedQuery);
        Assert.Single(result.Items);
        Assert.Equal(offers[2].Id, result.Items[0].Id);
        Assert.Equal(offers[2].Title, result.Items[0].Title);
        Assert.Equal(offers[2].DiscountPercentage, result.Items[0].DiscountPercentage);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public async Task InvalidQueryDoesNotCallRepository()
    {
        var repository = new FakeOfferRepository();
        await Assert.ThrowsAsync<ApplicationValidationException>(() => new GetOffersService(repository).ExecuteAsync(new GetOffersQuery(Page: 0)));
        Assert.Equal(0, repository.GetAllAsyncCallCount);
    }

    [Fact]
    public async Task EmptyResultPreservesPagination()
    {
        var repository = new FakeOfferRepository();
        var result = await new GetOffersService(repository).ExecuteAsync(new GetOffersQuery());
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
    }
}
