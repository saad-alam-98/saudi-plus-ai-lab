using SaudiPlus.IntegrationTests;
using SaudiPlus.Application.Common.Models;
using SaudiPlus.Application.Offers.CreateOffer;
using SaudiPlus.Application.Offers.GetOffer;
using System.Net;
using System.Net.Http.Json;

namespace SaudiPlus.IntegrationTests.Controllers;

public class GetOffersControllerTests
{
    [Fact]
    public async Task CollectionFiltersSortsAndPaginates()
    {
        using var factory = new InMemoryOfferWebApplicationFactory();
        using var client = factory.CreateClient();
        var prefix = $"test-{Guid.NewGuid():N}-";
        var date = DateTime.UtcNow;
        var data = new[] { ("C", 20m), ("A", 40m), ("B", 40m), ("D", 60m), ("E", 80m) };
        foreach (var (title, discount) in data)
        {
            var response = await client.PostAsJsonAsync("/api/offers", new CreateOfferRequest(Guid.NewGuid(), prefix + title, discount, date, date.AddDays(1)));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        async Task<PaginatedResult<OfferResponse>> Get(string parameters)
        {
            var response = await client.GetAsync($"/api/offers?search={prefix}&{parameters}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<PaginatedResult<OfferResponse>>())!;
        }

        var all = await Get("sortBy=title&sortDirection=asc");
        Assert.Equal(new[] { "A", "B", "C", "D", "E" }, all.Items.Select(x => x.Title[prefix.Length..]));
        Assert.Equal(5, all.TotalCount);
        Assert.Equal(1, all.TotalPages);
        var descending = await Get("sortBy=title&sortDirection=desc");
        Assert.Equal(new[] { "E", "D", "C", "B", "A" }, descending.Items.Select(x => x.Title[prefix.Length..]));

        var filtered = await Get("minDiscount=40&sortBy=title&sortDirection=asc&page=2&pageSize=2");
        Assert.Equal(new[] { "D", "E" }, filtered.Items.Select(x => x.Title[prefix.Length..]));
        Assert.Equal(4, filtered.TotalCount);
        Assert.Equal(2, filtered.TotalPages);
        Assert.False(filtered.HasNextPage);
        Assert.True(filtered.HasPreviousPage);

        var first = await Get("sortBy=title&page=1&pageSize=2&sortDirection=asc");
        var middle = await Get("sortBy=title&page=2&pageSize=2&sortDirection=asc");
        var last = await Get("sortBy=title&page=3&pageSize=2&sortDirection=asc");
        var beyond = await Get("sortBy=title&page=4&pageSize=2&sortDirection=asc");
        Assert.Equal(new[] { "A", "B" }, first.Items.Select(x => x.Title[prefix.Length..]));
        Assert.Equal(new[] { "C", "D" }, middle.Items.Select(x => x.Title[prefix.Length..]));
        Assert.Equal("E", Assert.Single(last.Items).Title[prefix.Length..]);
        Assert.Empty(beyond.Items);
        foreach (var page in new[] { first, middle, last, beyond }) { Assert.Equal(5, page.TotalCount); Assert.Equal(3, page.TotalPages); }
        Assert.True(first.HasNextPage); Assert.False(first.HasPreviousPage);
        Assert.True(middle.HasNextPage); Assert.True(middle.HasPreviousPage);
        Assert.False(last.HasNextPage); Assert.True(last.HasPreviousPage);
        Assert.False(beyond.HasNextPage); Assert.True(beyond.HasPreviousPage);

        var ties = await Get("minDiscount=40&sortBy=discount&sortDirection=asc");
        Assert.Equal(ties.Items.Where(x => x.DiscountPercentage == 40).Select(x => x.Id).OrderBy(x => x), ties.Items.Where(x => x.DiscountPercentage == 40).Select(x => x.Id));
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=101")]
    [InlineData("minDiscount=101")]
    [InlineData("sortBy=price")]
    [InlineData("sortDirection=random")]
    [InlineData("search=aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("page=abc")]
    [InlineData("pageSize=abc")]
    [InlineData("minDiscount=abc")]
    public async Task InvalidParametersReturnBadRequest(string parameters)
    {
        using var factory = new InMemoryOfferWebApplicationFactory();
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/offers?" + parameters);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
