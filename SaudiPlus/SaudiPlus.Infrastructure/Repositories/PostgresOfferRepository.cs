using Microsoft.EntityFrameworkCore;
using SaudiPlus.Application.Common.Models;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Application.Offers.GetOffers;
using SaudiPlus.Domain.Entities;
using SaudiPlus.Infrastructure.Persistence;

namespace SaudiPlus.Infrastructure.Repositories;

public sealed class PostgresOfferRepository(SaudiPlusDbContext context) : IOfferRepository
{
    public async Task AddAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        await context.Offers.AddAsync(offer, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<Offer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Offers.AsNoTracking().FirstOrDefaultAsync(offer => offer.Id == id, cancellationToken);

    public async Task<PaginatedResult<Offer>> GetAllAsync(
        GetOffersQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Offer> offers = context.Offers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var escaped = query.Search.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var pattern = $"%{escaped}%";
            offers = offers.Where(offer => EF.Functions.ILike(offer.Title, pattern, "\\"));
        }

        if (query.MinDiscount.HasValue)
            offers = offers.Where(offer => offer.DiscountPercentage >= query.MinDiscount.Value);

        var totalCount = await offers.CountAsync(cancellationToken);
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        offers = query.SortBy.ToLowerInvariant() switch
        {
            "discount" => descending
                ? offers.OrderByDescending(offer => offer.DiscountPercentage).ThenBy(offer => offer.Id)
                : offers.OrderBy(offer => offer.DiscountPercentage).ThenBy(offer => offer.Id),
            "title" => descending
                ? offers.OrderByDescending(offer => offer.Title).ThenBy(offer => offer.Id)
                : offers.OrderBy(offer => offer.Title).ThenBy(offer => offer.Id),
            _ => descending
                ? offers.OrderByDescending(offer => offer.StartDate).ThenBy(offer => offer.Id)
                : offers.OrderBy(offer => offer.StartDate).ThenBy(offer => offer.Id)
        };

        var offset = checked((int)(((long)query.Page - 1) * query.PageSize));
        var items = await offers.Skip(offset).Take(query.PageSize).ToListAsync(cancellationToken);
        return new PaginatedResult<Offer>(items, query.Page, query.PageSize, totalCount);
    }
}
