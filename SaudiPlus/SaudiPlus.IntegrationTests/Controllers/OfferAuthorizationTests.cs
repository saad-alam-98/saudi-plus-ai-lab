using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Application.Offers.CreateOffer;
namespace SaudiPlus.IntegrationTests.Controllers;
public class OfferAuthorizationTests
{
    [Fact]
    public async Task AnonymousIsChallengedAndCustomerIsForbiddenWhileReadsStayPublic()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var start = DateTime.UtcNow;
        var request = new CreateOfferRequest(Guid.NewGuid(), "Offer", 20m, start, start.AddDays(1));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/offers", request)).StatusCode);
        var token = factory.Services.GetRequiredService<IJwtTokenService>().Create(Guid.NewGuid(), "customer@example.test", ["Customer"]);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/offers", request)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/offers")).StatusCode);
    }
}
