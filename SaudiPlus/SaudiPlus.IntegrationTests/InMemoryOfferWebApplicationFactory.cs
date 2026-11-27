using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Infrastructure.Repositories;
namespace SaudiPlus.IntegrationTests;
public sealed class InMemoryOfferWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.ConfigureServices(services =>
    {
        services.RemoveAll<IOfferRepository>();
        services.AddScoped<IOfferRepository, InMemoryOfferRepository>();
        services.RemoveAll<IMerchantAccess>();
        services.AddScoped<IMerchantAccess, TrustedMerchantAccess>();
        services.AddAuthentication(options => { options.DefaultAuthenticateScheme = "Test"; options.DefaultChallengeScheme = "Test"; })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
    });
    private sealed class TrustedMerchantAccess : IMerchantAccess
    {
        public Task<bool> ExistsAsync(Guid id, CancellationToken token = default) => Task.FromResult(true);
        public Task<bool> IsMemberAsync(Guid user, Guid id, CancellationToken token = default) => Task.FromResult(true);
    }
    private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim("sub", Guid.NewGuid().ToString()), new Claim("role", "Administrator") };
            var identity = new ClaimsIdentity(claims, "Test", "sub", "role");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
        }
    }
}
