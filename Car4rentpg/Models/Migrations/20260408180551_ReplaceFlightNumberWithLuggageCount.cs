using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceFlightNumberWithLuggageCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlightNumber",
                table: "TransferBookings");

            migrationBuilder.AddColumn<int>(
                name: "LuggageCount",
                table: "TransferBookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LuggageCount",
                table: "TransferBookings");

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                table: "TransferBookings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
