using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Application.Offers.GetOffer;
using SaudiPlus.Application.Offers.CreateOffer;

namespace SaudiPlus.Api.Controllers
{
    [ApiController]
    [Route("api/offers")]
    public class OffersController : ControllerBase
    {
        private readonly CreateOfferService _createOfferService;
        private readonly GetOfferService _getOfferService;

        public OffersController(
            CreateOfferService createOfferService,
            GetOfferService getOfferService
        )
        {
            _createOfferService = createOfferService;
            _getOfferService = getOfferService;
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

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OfferResponse>> GetById(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var offer = await _getOfferService.ExecuteAsync(
                id, cancellationToken);

            if (offer is null)
                return NotFound();

            return Ok(offer);
        }
    }
}
