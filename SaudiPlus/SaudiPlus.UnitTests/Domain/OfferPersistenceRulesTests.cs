using SaudiPlus.Domain.Entities;
using SaudiPlus.Domain.Exceptions;

namespace SaudiPlus.UnitTests.Domain;

public class OfferPersistenceRulesTests
{
    private static readonly DateTime Start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void TitleAtLimitIsAccepted() => Assert.Equal(200, Create(new string('x', 200), 10m, Start, Start.AddDays(1)).Title.Length);

    [Fact]
    public void TitlePastLimitIsRejected() => Rejects("title must not exceed 200", () => Create(new string('x', 201), 10m, Start, Start.AddDays(1)));

    [Theory]
    [InlineData("0.01")]
    [InlineData("12.34")]
    [InlineData("12.3400")]
    [InlineData("100.00")]
    public void RepresentableDiscountIsAccepted(string value) => Assert.Equal(decimal.Parse(value), Create("Valid", decimal.Parse(value), Start, Start.AddDays(1)).DiscountPercentage);

    [Theory]
    [InlineData("0.001")]
    [InlineData("12.345")]
    [InlineData("99.999")]
    public void DiscountRequiringRoundingIsRejected(string value) => Rejects("at most two decimal places", () => Create("Invalid", decimal.Parse(value), Start, Start.AddDays(1)));

    [Theory]
    [InlineData(DateTimeKind.Local, DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified, DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Utc, DateTimeKind.Local)]
    [InlineData(DateTimeKind.Utc, DateTimeKind.Unspecified)]
    public void NonUtcDatesAreRejected(DateTimeKind startKind, DateTimeKind endKind) =>
        Rejects("dates must be UTC", () => Create("Invalid", 10m, DateTime.SpecifyKind(Start, startKind), DateTime.SpecifyKind(Start.AddDays(1), endKind)));

    [Fact]
    public void UtcDatesAreAccepted() => Assert.Equal(DateTimeKind.Utc, Create("Valid", 10m, Start, Start.AddDays(1)).EndDate.Kind);

    private static Offer Create(string title, decimal discount, DateTime start, DateTime end) => new(Guid.NewGuid(), title, discount, start, end);
    private static void Rejects(string message, Action action) => Assert.Contains(message, Assert.Throws<DomainValidationException>(action).Message);
}
