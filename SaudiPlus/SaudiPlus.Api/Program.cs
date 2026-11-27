using SaudiPlus.Api;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Infrastructure.Repositories;
using SaudiPlus.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SaudiPlus.Infrastructure.Identity;
using SaudiPlus.Api.ExceptionHandling;
using SaudiPlus.Application.Offers.CreateOffer;
using SaudiPlus.Application.Offers.GetOffer;
using SaudiPlus.Api.Options;
using SaudiPlus.Application.Offers.GetOffers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SaudiPlusDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentityCore<IdentityUser<Guid>>(options =>
    options.User.RequireUniqueEmail = true)
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<SaudiPlusDbContext>();
builder.Services.AddScoped<IOfferRepository, PostgresOfferRepository>();
builder.Services.AddScoped<IMerchantAccess, PostgresMerchantAccess>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddAuthorizationBuilder().AddPolicy("CanCreateOffer", policy => policy.RequireAuthenticatedUser().RequireRole("Merchant", "Administrator"));
builder.Services.AddScoped<CreateOfferService>();
builder.Services.AddScoped<GetOfferService>();
builder.Services.AddScoped<GetOffersService>();

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddOptions<JwtOptions>().BindConfiguration("Jwt")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer) && !string.IsNullOrWhiteSpace(o.Audience), "JWT issuer and audience are required.")
    .Validate(o => System.Text.Encoding.UTF8.GetByteCount(o.SigningKey) >= 32, "JWT signing key must be at least 256 bits.")
    .Validate(o => o.LifetimeMinutes == 15, "JWT lifetime must be 15 minutes.").ValidateOnStart();
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddOptions<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>("Bearer")
    .Configure<Microsoft.Extensions.Options.IOptions<JwtOptions>>((options, jwt) =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = jwt.Value.ValidationParameters();
    });
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, context, cancellationToken) =>
{
    document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
    document.Components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();
    document.Components.SecuritySchemes["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT"
    };
    document.Security = [new Microsoft.OpenApi.OpenApiSecurityRequirement { [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document)] = [] }, new Microsoft.OpenApi.OpenApiSecurityRequirement()];
    return Task.CompletedTask;
}));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOptions<AIServiceOptions>()
    .BindConfiguration(AIServiceOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

if (args.Contains("--initialize-roles", StringComparer.Ordinal))
{
    await using var scope = app.Services.CreateAsyncScope();
    await RoleInitializer.InitializeAsync(
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
    return;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Saudi Plus API v1");
    });
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
