using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class AddFullPaymentFieldsToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FullyPaidAt",
                table: "Bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFullyPaid",
                table: "Bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullyPaidAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsFullyPaid",
                table: "Bookings");
        }
    }
}
