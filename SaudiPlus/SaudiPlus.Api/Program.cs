using SaudiPlus.Application.Interfaces;
using SaudiPlus.Infrastructure.Repositories;
using SaudiPlus.Api.ExceptionHandling;
using SaudiPlus.Application.Offers.CreateOffer;
using SaudiPlus.Application.Offers.GetOffer;
using SaudiPlus.Api.Options;
using SaudiPlus.Application.Offers.GetOffers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IOfferRepository, InMemoryOfferRepository>();
builder.Services.AddScoped<CreateOfferService>();
builder.Services.AddScoped<GetOfferService>();
builder.Services.AddScoped<GetOffersService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOptions<AIServiceOptions>()
    .BindConfiguration(AIServiceOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}