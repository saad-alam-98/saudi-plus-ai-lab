using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Application.Common.Models;
using SaudiPlus.Application.Offers.CreateOffer;
using SaudiPlus.Application.Offers.GetOffer;
using SaudiPlus.Application.Offers.GetOffers;

namespace SaudiPlus.Api.Controllers
{
    [ApiController]
    [Route("api/offers")]
    public class OffersController : ControllerBase
    {
        private readonly CreateOfferService _createOfferService;
        private readonly GetOfferService _getOfferService;
        private readonly GetOffersService _getOffersService;

        public OffersController(
            CreateOfferService createOfferService,
            GetOfferService getOfferService,
            GetOffersService getOffersService
        )
        {
            _createOfferService = createOfferService;
            _getOfferService = getOfferService;
            _getOffersService = getOffersService;
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

            return CreatedAtAction(
                nameof(GetById),
                new { id = offerId },
                new { Id = offerId }
            );
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

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<OfferResponse>>> GetAll(
            [FromQuery] GetOffersQuery query,
            CancellationToken cancellationToken
        )
        {
            var offers = await _getOffersService.ExecuteAsync(
                query,
                cancellationToken
            );

            return Ok(offers);
        }
    }
}
