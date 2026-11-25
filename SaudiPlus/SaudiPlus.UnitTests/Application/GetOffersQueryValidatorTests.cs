using SaudiPlus.Application.Common.Exceptions;
using SaudiPlus.Application.Offers.GetOffers;

namespace SaudiPlus.UnitTests.Application;

public class GetOffersQueryValidatorTests
{
    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(-100)]
    public void RejectsInvalidPage(int value) => Invalid(new GetOffersQuery(Page: value), "Page must");

    [Theory]
    [InlineData(-1)] [InlineData(0)] [InlineData(101)] [InlineData(1000000)]
    public void RejectsInvalidPageSize(int value) => Invalid(new GetOffersQuery(PageSize: value), "Page size");

    [Theory]
    [InlineData(1)] [InlineData(20)] [InlineData(100)]
    public void AcceptsPageSize(int value) => GetOffersQueryValidator.Validate(new GetOffersQuery(PageSize: value));

    [Theory]
    [InlineData(-1)] [InlineData(101)] [InlineData(150)]
    public void RejectsInvalidMinimumDiscount(int value) => Invalid(new GetOffersQuery(MinDiscount: (decimal)value), "Minimum discount");

    [Theory]
    [InlineData(null)] [InlineData(0)] [InlineData(50)] [InlineData(100)]
    public void AcceptsMinimumDiscount(int? value) => GetOffersQueryValidator.Validate(new GetOffersQuery(MinDiscount: (decimal?)value));

    [Theory]
    [InlineData("price")] [InlineData("unknown")] [InlineData("")] [InlineData(null)]
    public void RejectsInvalidSortField(string? value) => Invalid(new GetOffersQuery(SortBy: value!), "Unsupported sorting field");

    [Theory]
    [InlineData("discount")] [InlineData("title")] [InlineData("startDate")] [InlineData("DISCOUNT")]
    public void AcceptsSortField(string value) => GetOffersQueryValidator.Validate(new GetOffersQuery(SortBy: value));

    [Theory]
    [InlineData("random")] [InlineData("ascending")] [InlineData("")] [InlineData(null)]
    public void RejectsInvalidDirection(string? value) => Invalid(new GetOffersQuery(SortDirection: value!), "Sort direction");

    [Theory]
    [InlineData("asc")] [InlineData("desc")] [InlineData("ASC")] [InlineData("DESC")]
    public void AcceptsDirection(string value) => GetOffersQueryValidator.Validate(new GetOffersQuery(SortDirection: value));

    [Fact]
    public void RejectsOverflow() => Invalid(new GetOffersQuery(Page: int.MaxValue, PageSize: 100), "supported range");

    [Fact]
    public void AcceptsSearchAtLimit() => GetOffersQueryValidator.Validate(new GetOffersQuery(Search: new string('a', 100)));

    [Fact]
    public void RejectsSearchPastLimit() => Invalid(new GetOffersQuery(Search: new string('a', 101)), "Search must");

    [Fact]
    public void AcceptsDefaultQuery() => GetOffersQueryValidator.Validate(new GetOffersQuery());

    private static void Invalid(GetOffersQuery query, string message) =>
        Assert.Contains(message, Assert.Throws<ApplicationValidationException>(() => GetOffersQueryValidator.Validate(query)).Message);
}
