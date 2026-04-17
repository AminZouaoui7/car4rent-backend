using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingOptionsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BabySeatAmount",
                table: "Bookings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BabySeatPercentage",
                table: "Bookings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBabySeat",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSecondDriver",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecondDriverFirstName",
                table: "Bookings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondDriverLastName",
                table: "Bookings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondDriverPhone",
                table: "Bookings",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BabySeatAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BabySeatPercentage",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HasBabySeat",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HasSecondDriver",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SecondDriverFirstName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SecondDriverLastName",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SecondDriverPhone",
                table: "Bookings");
        }
    }
}
