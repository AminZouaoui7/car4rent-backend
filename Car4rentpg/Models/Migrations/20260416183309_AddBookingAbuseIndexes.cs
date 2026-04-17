using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingAbuseIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Email_CreatedAt",
                table: "Bookings",
                columns: new[] { "Email", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Email_Status",
                table: "Bookings",
                columns: new[] { "Email", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Email_VehicleId_StartDate_EndDate_CreatedAt",
                table: "Bookings",
                columns: new[] { "Email", "VehicleId", "StartDate", "EndDate", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_Email_CreatedAt",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_Email_Status",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_Email_VehicleId_StartDate_EndDate_CreatedAt",
                table: "Bookings");
        }
    }
}
