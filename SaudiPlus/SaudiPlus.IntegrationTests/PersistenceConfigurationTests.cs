using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaudiPlus.Domain.Entities;
using SaudiPlus.Infrastructure.Persistence;

namespace SaudiPlus.IntegrationTests;

public class PersistenceConfigurationTests
{
    [Fact]
    public void DbContextResolvesAndOfferModelUsesDomainConstructor()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var scope = factory.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<SaudiPlusDbContext>();
        var entity = context.Model.FindEntityType(typeof(Offer));

        Assert.NotNull(entity);
        Assert.Equal("offers", entity.GetTableName());
        Assert.NotNull(entity.ConstructorBinding);
        Assert.Equal(5, entity.ConstructorBinding.ParameterBindings.Count);
        Assert.Equal("id", entity.FindProperty(nameof(Offer.Id))!.GetColumnName());
        Assert.Equal(200, entity.FindProperty(nameof(Offer.Title))!.GetMaxLength());
        Assert.Equal(5, entity.FindProperty(nameof(Offer.DiscountPercentage))!.GetPrecision());
        Assert.Equal(2, entity.FindProperty(nameof(Offer.DiscountPercentage))!.GetScale());
    }
}
