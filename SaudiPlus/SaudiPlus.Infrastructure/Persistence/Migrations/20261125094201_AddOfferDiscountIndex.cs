using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaudiPlus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferDiscountIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_offers_discount_id",
                table: "offers",
                columns: new[] { "discount_percentage", "id" },
                descending: new[] { true, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_offers_discount_id",
                table: "offers");
        }
    }
}
