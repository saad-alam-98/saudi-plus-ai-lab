using System.Net.Http.Json;
using SaudiPlus.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SaudiPlus.Infrastructure.Identity;
using SaudiPlus.Infrastructure.Persistence;

namespace SaudiPlus.IntegrationTests;

public class IdentityPersistenceTests
{
    [Fact]
    public async Task RolesAreIdempotentAndGuidUsersPersistWithUniqueEmails()
    {
        using var startup = new WebApplicationFactory<Program>();
        using var startupScope = startup.Services.CreateScope();
        var configured = startupScope.ServiceProvider.GetRequiredService<SaudiPlusDbContext>().Database.GetConnectionString();
        var connection = new NpgsqlConnectionStringBuilder(configured);
        Assert.Equal("saudi_plus", connection.Database);
        Assert.Equal(5433, connection.Port);

        const string testDatabase = "saudi_plus_identity_tests";
        var admin = new NpgsqlConnectionStringBuilder(configured) { Database = "postgres" };
        await using (var adminConnection = new NpgsqlConnection(admin.ConnectionString))
        {
            await adminConnection.OpenAsync();
            await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'saudi_plus_identity_tests'", adminConnection);
            if (await exists.ExecuteScalarAsync() is null)
            {
                await using var create = new NpgsqlCommand("CREATE DATABASE saudi_plus_identity_tests", adminConnection);
                await create.ExecuteNonQueryAsync();
            }
        }

        connection.Database = testDatabase;
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = connection.ConnectionString
                })));
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<SaudiPlusDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await RoleInitializer.InitializeAsync(roleManager);
        await RoleInitializer.InitializeAsync(roleManager);
        var roleNames = await context.Roles.Select(role => role.Name).ToListAsync();
        Assert.Equal(3, roleNames.Count);
        Assert.Equal(IdentityRoles.All.OrderBy(name => name), roleNames.OrderBy(name => name));

        var userManager = services.GetRequiredService<UserManager<IdentityUser<Guid>>>();
        Assert.True(userManager.Options.User.RequireUniqueEmail);
        var email = $"identity-{Guid.NewGuid():N}@example.test";
        var user = new IdentityUser<Guid> { UserName = email, Email = email };
        Assert.True((await userManager.CreateAsync(user)).Succeeded);
        Assert.NotEqual(Guid.Empty, user.Id);
        var duplicate = new IdentityUser<Guid> { UserName = "other-" + email, Email = email };
        var duplicateResult = await userManager.CreateAsync(duplicate);
        Assert.False(duplicateResult.Succeeded);
        Assert.Contains(duplicateResult.Errors, error => error.Code == "DuplicateEmail");

                var identity = services.GetRequiredService<IIdentityService>();
        var registrationEmail = $"auth-{Guid.NewGuid():N}@example.test";
        const string password = "Test-only_StrongPassword!42";
        Assert.False(await identity.RegisterAsync(registrationEmail, "weak"));
        Assert.Null(await userManager.FindByEmailAsync(registrationEmail));
        Assert.True(await identity.RegisterAsync(registrationEmail, password));
        Assert.False(await identity.RegisterAsync(registrationEmail, password));
        var registered = await userManager.FindByEmailAsync(registrationEmail);
        Assert.NotNull(registered);
        Assert.NotEqual(password, registered.PasswordHash);
        Assert.True(await userManager.CheckPasswordAsync(registered, password));
        Assert.Equal(new[] { IdentityRoles.Customer }, await userManager.GetRolesAsync(registered));
        var access = await identity.LoginAsync(registrationEmail, password);
        Assert.NotNull(access);
        var settings = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<JwtOptions>>().Value;
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(access.Token, settings.ValidationParameters(), out _);
        Assert.Equal(registered.Id.ToString(), principal.FindFirst("sub")!.Value);
        Assert.Equal(registrationEmail, principal.FindFirst("email")!.Value);
        Assert.Equal(IdentityRoles.Customer, principal.FindFirst("role")!.Value);
        Assert.NotNull(principal.FindFirst("jti"));
        var wrongIssuer = settings.ValidationParameters();
        wrongIssuer.ValidIssuer = "wrong";
        Assert.Throws<Microsoft.IdentityModel.Tokens.SecurityTokenInvalidIssuerException>(() => handler.ValidateToken(access.Token, wrongIssuer, out _));
        var wrongAudience = settings.ValidationParameters();
        wrongAudience.ValidAudience = "wrong";
        Assert.Throws<Microsoft.IdentityModel.Tokens.SecurityTokenInvalidAudienceException>(() => handler.ValidateToken(access.Token, wrongAudience, out _));
        var expired = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(settings.Issuer, settings.Audience,
            notBefore: DateTime.UtcNow.AddMinutes(-20), expires: DateTime.UtcNow.AddMinutes(-5),
            signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(settings.ValidationParameters().IssuerSigningKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));
        Assert.Throws<Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException>(() => handler.ValidateToken(handler.WriteToken(expired), settings.ValidationParameters(), out _));
        Assert.InRange((access.ExpiresAtUtc - DateTime.UtcNow).TotalMinutes, 14, 15);
        var wrongKey = settings.ValidationParameters();
        wrongKey.IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(new byte[32]);
        Assert.ThrowsAny<Microsoft.IdentityModel.Tokens.SecurityTokenException>(() => handler.ValidateToken(access.Token, wrongKey, out _));
        for (var attempt = 0; attempt < 5; attempt++) Assert.Null(await identity.LoginAsync(registrationEmail, "incorrect"));
        Assert.True(await userManager.IsLockedOutAsync(registered));
        Assert.Null(await identity.LoginAsync(registrationEmail, password));
        Assert.Null(await identity.LoginAsync("missing@example.test", password));

        using var client = factory.CreateClient();
        var httpEmail = $"http-{Guid.NewGuid():N}@example.test";
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email = httpEmail, password, roles = new[] { "Administrator" } });
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var httpUser = await userManager.FindByEmailAsync(httpEmail);
        Assert.Equal(new[] { IdentityRoles.Customer }, await userManager.GetRolesAsync(httpUser!));
        Assert.Equal(System.Net.HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/login", new { email = httpEmail, password })).StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/auth/login", new { email = httpEmail, password = "wrong" })).StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/register", new { email = "invalid", password })).StatusCode);
        var merchant = new SaudiPlus.Domain.Entities.Merchant("Trusted test merchant");
        var owner = new IdentityUser<Guid> { UserName = $"owner-{Guid.NewGuid():N}", Email = $"owner-{Guid.NewGuid():N}@example.test" };
        var outsider = new IdentityUser<Guid> { UserName = $"outsider-{Guid.NewGuid():N}", Email = $"outsider-{Guid.NewGuid():N}@example.test" };
        var administrator = new IdentityUser<Guid> { UserName = $"admin-{Guid.NewGuid():N}", Email = $"admin-{Guid.NewGuid():N}@example.test" };
        foreach (var fixture in new[] { owner, outsider, administrator })
            Assert.True((await userManager.CreateAsync(fixture)).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(owner, IdentityRoles.Merchant)).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(outsider, IdentityRoles.Merchant)).Succeeded);
        Assert.True((await userManager.AddToRoleAsync(administrator, IdentityRoles.Administrator)).Succeeded);
        context.Merchants.Add(merchant);
        context.MerchantMemberships.Add(new SaudiPlus.Domain.Entities.MerchantMembership(owner.Id, merchant.Id));
        await context.SaveChangesAsync();
        var tokenService = services.GetRequiredService<IJwtTokenService>();
        var offerStart = DateTime.UtcNow;
        var offerRequest = new SaudiPlus.Application.Offers.CreateOffer.CreateOfferRequest(
            merchant.Id, $"Owned-{Guid.NewGuid():N}", 20m, offerStart, offerStart.AddDays(1));
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
            tokenService.Create(owner.Id, owner.Email!, [IdentityRoles.Merchant]).Token);
        Assert.Equal(System.Net.HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/offers", offerRequest)).StatusCode);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
            tokenService.Create(outsider.Id, outsider.Email!, [IdentityRoles.Merchant]).Token);
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/offers", offerRequest)).StatusCode);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",
            tokenService.Create(administrator.Id, administrator.Email!, [IdentityRoles.Administrator]).Token);
        Assert.Equal(System.Net.HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/offers", offerRequest)).StatusCode);
        Assert.Equal(2, await context.Offers.CountAsync(offer => offer.Title == offerRequest.Title));
        await using var reloaded = new SaudiPlusDbContext(
            new DbContextOptionsBuilder<SaudiPlusDbContext>().UseNpgsql(connection.ConnectionString).Options);
        Assert.Equal(user.Id, (await reloaded.Users.AsNoTracking().SingleAsync(found => found.Email == email)).Id);
    }
}
