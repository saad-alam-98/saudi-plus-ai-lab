using Microsoft.AspNetCore.Mvc;
using SaudiPlus.IntegrationTests;
using SaudiPlus.Application.Offers.CreateOffer;
using SaudiPlus.Application.Offers.GetOffer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace SaudiPlus.IntegrationTests.Controllers
{
    public class OffersControllerTests : IClassFixture<InMemoryOfferWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public OffersControllerTests(
            InMemoryOfferWebApplicationFactory factory
        )
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Create_ShouldReturn201_WhenRequestIsValid()
        {
            var startDate = DateTime.UtcNow;

            var request = new CreateOfferRequest(
                Guid.NewGuid(),
                "20% Discount",
                20,
                startDate,
                startDate.AddDays(30)
            );

            var response = await _client.PostAsJsonAsync(
                "/api/offers",
                request
            );

            Assert.Equal(
                HttpStatusCode.Created,
                response.StatusCode
            );
        }

        [Fact]
        public async Task Create_ShouldReturn400_WhenDiscountExceeds100()
        {
            var startDate = DateTime.UtcNow;

            var request = new CreateOfferRequest(
                Guid.NewGuid(),
                "Invalid Discount",
                150,
                startDate,
                startDate.AddDays(30)
            );

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/offers",
                request);

            Assert.Equal(
               HttpStatusCode.BadRequest,
               response.StatusCode
            );

            var problem = await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);
            Assert.Equal(400, problem.Status);
            Assert.Equal("Invalid request", problem.Title);
            Assert.Equal(
                "Discount must be between 0 and 100.",
                problem.Detail
            );
        }

        [Fact]
        public async Task Create_ShouldReturn400_WhenEndDateIsBeforeStartDate()
        {
            var startDate = DateTime.UtcNow;

            var request = new CreateOfferRequest(
                Guid.NewGuid(),
                "Offer With Invalid Date",
                50,
                startDate,
                startDate.AddDays(-1)
            );

            var response = await _client.PostAsJsonAsync(
                "/api/offers",
                request
            );

            Assert.Equal(
               HttpStatusCode.BadRequest,
               response.StatusCode
            );

            var problem = await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problem);
            Assert.Equal(400, problem.Status);
            Assert.Equal("Invalid request", problem.Title);
            Assert.Equal(
                "End date must be after start date.",
                problem.Detail
            );
        }

        [Fact]
        public async Task CreateAndGet_ShouldReturnCreatedOffer()
        {
            // Arrange
            var startDate = DateTime.UtcNow;

            var request = new CreateOfferRequest(
               Guid.NewGuid(),
               "Electronics Discount",
               25,
               startDate,
               startDate.AddDays(30)
            );

            // Act - Create offer
            var createResponse = await _client.PostAsJsonAsync(
                "/api/offers",
                request);

            // Assert - Creation
            Assert.Equal(
                HttpStatusCode.Created,
                createResponse.StatusCode);

            var location = createResponse.Headers.Location;

            Assert.NotNull(location);

            var getResponse = await _client.GetAsync(location);

            Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode
            );

            var offer = await getResponse.Content
                .ReadFromJsonAsync<OfferResponse>();

            Assert.NotNull(offer);
            Assert.NotEqual(Guid.Empty, offer.Id);
            Assert.Equal(request.Title, offer.Title);
            Assert.Equal(
                request.DiscountPercentage,
                offer.DiscountPercentage);
        }

        [Fact]
        public async Task GetById_ShouldReturn404_WhenOfferDoesNotExist()
        {
            var nonExistentOfferId = Guid.NewGuid();
            var response = await _client.GetAsync(
                $"/api/offers/{nonExistentOfferId}");

            Assert.Equal(
                HttpStatusCode.NotFound,
                response.StatusCode
            );
        }
    }
}
