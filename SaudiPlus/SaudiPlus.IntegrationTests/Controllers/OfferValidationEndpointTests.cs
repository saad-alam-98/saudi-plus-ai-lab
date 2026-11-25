using Microsoft.AspNetCore.Mvc;
using SaudiPlus.IntegrationTests;
using SaudiPlus.Application.Offers.CreateOffer;
using System.Net;
using System.Net.Http.Json;

namespace SaudiPlus.IntegrationTests.Controllers;

public class OfferValidationEndpointTests
{
    [Fact]
    public async Task ValidUtcRequestIsAccepted()
    {
        using var factory = new InMemoryOfferWebApplicationFactory();
        using var client = factory.CreateClient();
        var start = DateTime.UtcNow;
        var response = await client.PostAsJsonAsync("/api/offers", new CreateOfferRequest(Guid.NewGuid(), new string('x', 200), 12.34m, start, start.AddDays(1)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("title")]
    [InlineData("precision")]
    [InlineData("utc")]
    public async Task InvalidDomainInputReturnsBadRequest(string scenario)
    {
        using var factory = new InMemoryOfferWebApplicationFactory();
        using var client = factory.CreateClient();
        var start = DateTime.UtcNow;
        var request = new CreateOfferRequest(Guid.NewGuid(), scenario == "title" ? new string('x', 201) : "Valid", scenario == "precision" ? 12.345m : 12.34m, scenario == "utc" ? DateTime.SpecifyKind(start, DateTimeKind.Unspecified) : start, start.AddDays(1));
        var response = await client.PostAsJsonAsync("/api/offers", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Invalid request", problem.Title);
        Assert.Contains(scenario switch { "title" => "200 characters", "precision" => "two decimal places", _ => "UTC" }, problem.Detail);
    }
}
