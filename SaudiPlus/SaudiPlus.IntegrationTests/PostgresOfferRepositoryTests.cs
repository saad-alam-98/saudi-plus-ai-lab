using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using SaudiPlus.Application.Common.Models;
using SaudiPlus.Application.Offers.CreateOffer;
using SaudiPlus.Application.Offers.GetOffer;
using SaudiPlus.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;

namespace SaudiPlus.IntegrationTests;

public class PostgresOfferRepositoryTests
{
    [Fact]
    public async Task OffersPersistAcrossApiRestartsAndQueriesRunInPostgres()
    {
        using var startup = new WebApplicationFactory<Program>();
        using var scope = startup.Services.CreateScope();
        var configured = scope.ServiceProvider.GetRequiredService<SaudiPlusDbContext>().Database.GetConnectionString();
        var connection = new NpgsqlConnectionStringBuilder(configured);
        Assert.Equal("saudi_plus", connection.Database);
        Assert.Equal(5433, connection.Port);
        connection.Database = "saudi_plus_goal4_tests";
        var testConnection = connection.ConnectionString;

        var admin = new NpgsqlConnectionStringBuilder(configured) { Database = "postgres" };
        await using (var adminConnection = new NpgsqlConnection(admin.ConnectionString))
        {
            await adminConnection.OpenAsync();
            await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'saudi_plus_goal4_tests'", adminConnection);
            if (await exists.ExecuteScalarAsync() is null)
            {
                await using var create = new NpgsqlCommand("CREATE DATABASE saudi_plus_goal4_tests", adminConnection);
                await create.ExecuteNonQueryAsync();
            }
        }

        var options = new DbContextOptionsBuilder<SaudiPlusDbContext>().UseNpgsql(testConnection).Options;
        await using (var database = new SaudiPlusDbContext(options))
            await database.Database.MigrateAsync();

        WebApplicationFactory<Program> CreateFactory() => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<SaudiPlusDbContext>();
                services.RemoveAll<DbContextOptions<SaudiPlusDbContext>>();
                services.AddDbContext<SaudiPlusDbContext>(options => options.UseNpgsql(testConnection));
            }));

        var prefix = $"pg{Guid.NewGuid():N}%_";
        var decoy = prefix.Replace("%_", "AB") + "decoy";
        var slashTitle = prefix + "\\literal";
        var start = DateTime.UtcNow;
        var expected = new[] { ("C", 20m), ("A", 40m), ("B", 40m) };
        await using (var seed = new SaudiPlusDbContext(options))
        {
            foreach (var (suffix, discount) in expected)
                seed.Offers.Add(new SaudiPlus.Domain.Entities.Offer(Guid.NewGuid(), prefix + suffix, discount, start, start.AddDays(1)));
            seed.Offers.Add(new SaudiPlus.Domain.Entities.Offer(Guid.NewGuid(), decoy, 50m, start, start.AddDays(1)));
            seed.Offers.Add(new SaudiPlus.Domain.Entities.Offer(Guid.NewGuid(), slashTitle, 30m, start, start.AddDays(1)));
            await seed.SaveChangesAsync();
        }
        using (var first = CreateFactory())
        using (var firstClient = first.CreateClient())
        {
            var persisted = await firstClient.GetFromJsonAsync<PaginatedResult<OfferResponse>>(
                $"/api/offers?search={Uri.EscapeDataString(prefix)}");
            Assert.Equal(4, persisted!.TotalCount);
        }

        using var second = CreateFactory();
        using var restartedClient = second.CreateClient();
        var url = $"/api/offers?search={Uri.EscapeDataString(prefix)}&minDiscount=40&sortBy=title&sortDirection=asc&page=1&pageSize=1";
        var firstPage = await restartedClient.GetFromJsonAsync<PaginatedResult<OfferResponse>>(url);
        Assert.NotNull(firstPage);
        Assert.Equal(2, firstPage.TotalCount);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(prefix + "A", Assert.Single(firstPage.Items).Title);
        Assert.True(firstPage.HasNextPage);
        var secondPage = await restartedClient.GetFromJsonAsync<PaginatedResult<OfferResponse>>(url.Replace("page=1", "page=2"));
        Assert.NotNull(secondPage);
        Assert.Equal(prefix + "B", Assert.Single(secondPage.Items).Title);
        Assert.True(secondPage.HasPreviousPage);
        Assert.False(secondPage.HasNextPage);

        var literalSearch = await restartedClient.GetFromJsonAsync<PaginatedResult<OfferResponse>>(
            $"/api/offers?search={Uri.EscapeDataString((prefix + "\\").ToUpperInvariant())}");
        Assert.NotNull(literalSearch);
        Assert.Equal(slashTitle, Assert.Single(literalSearch.Items).Title);

        var tiedDiscounts = await restartedClient.GetFromJsonAsync<PaginatedResult<OfferResponse>>(
            $"/api/offers?search={Uri.EscapeDataString(prefix)}&minDiscount=40&sortBy=discount&sortDirection=desc");
        Assert.NotNull(tiedDiscounts);
        Assert.Equal(tiedDiscounts.Items.Select(offer => offer.Id).OrderBy(id => id),
            tiedDiscounts.Items.Select(offer => offer.Id));

        var byId = await restartedClient.GetAsync($"/api/offers/{firstPage.Items[0].Id}");
        Assert.Equal(HttpStatusCode.OK, byId.StatusCode);
        Assert.Equal(prefix + "A", (await byId.Content.ReadFromJsonAsync<OfferResponse>())!.Title);
    }
}
