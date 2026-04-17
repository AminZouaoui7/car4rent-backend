using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingExtraOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasBabySeat",
                table: "Bookings",
                newName: "HasProtectionPlus");

            migrationBuilder.RenameColumn(
                name: "BabySeatPercentage",
                table: "Bookings",
                newName: "SecondDriverAmount");

            migrationBuilder.AddColumn<int>(
                name: "BabySeatQuantity",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "BoosterSeatAmount",
                table: "Bookings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BoosterSeatQuantity",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ChildSeatAmount",
                table: "Bookings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChildSeatQuantity",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "FullTankAmount",
                table: "Bookings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GpsAmount",
                table: "Bookings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasFullTank",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasGps",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "ProtectionPlusAmount",
                table: "Bookings",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BabySeatQuantity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BoosterSeatAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BoosterSeatQuantity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ChildSeatAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ChildSeatQuantity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "FullTankAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "GpsAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HasFullTank",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HasGps",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ProtectionPlusAmount",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "SecondDriverAmount",
                table: "Bookings",
                newName: "BabySeatPercentage");

            migrationBuilder.RenameColumn(
                name: "HasProtectionPlus",
                table: "Bookings",
                newName: "HasBabySeat");
        }
    }
}
