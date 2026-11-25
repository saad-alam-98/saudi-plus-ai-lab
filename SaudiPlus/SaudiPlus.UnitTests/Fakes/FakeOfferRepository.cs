using SaudiPlus.Application.Common.Models;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Application.Offers.GetOffers;
using SaudiPlus.Domain.Entities;

namespace SaudiPlus.UnitTests.Fakes;

public class FakeOfferRepository : IOfferRepository
{
    private readonly List<Offer> _offers = new();
    public Offer? SavedOffer { get; private set; }
    public int GetAllAsyncCallCount { get; private set; }
    public GetOffersQuery? ReceivedQuery { get; private set; }

    public Task AddAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SavedOffer = offer;
        _offers.Add(offer);
        return Task.CompletedTask;
    }

    public Task<Offer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_offers.FirstOrDefault(offer => offer.Id == id));
    }

    public Task<PaginatedResult<Offer>> GetAllAsync(GetOffersQuery query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GetAllAsyncCallCount++;
        ReceivedQuery = query;
        var items = _offers.Skip((int)(((long)query.Page - 1) * query.PageSize)).Take(query.PageSize).ToList();
        return Task.FromResult(new PaginatedResult<Offer>(items, query.Page, query.PageSize, _offers.Count));
    }
}
