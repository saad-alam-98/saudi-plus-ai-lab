using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Infrastructure.Repositories;

namespace SaudiPlus.IntegrationTests;

public sealed class InMemoryOfferWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IOfferRepository>();
            services.AddScoped<IOfferRepository, InMemoryOfferRepository>();
        });
    }
}
