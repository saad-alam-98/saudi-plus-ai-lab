using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Application.Offers.CreateOffer;

namespace SaudiPlus.Api.Controllers
{
    [ApiController]
    [Route("api/offers")]
    public class OffersController : ControllerBase
    {
        private readonly CreateOfferService _createOfferService;

        public OffersController(CreateOfferService createOfferService)
        {
            _createOfferService = createOfferService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateOfferRequest request,
            CancellationToken cancellationToken
        )
        {
            var offerId = await _createOfferService.ExecuteAsync(
                request,
                cancellationToken);

            return Created($"/api/offers/{offerId}", new
            {
                Id = offerId
            });
        }
    }
}
