using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnCityToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "TotalPrice",
                table: "Bookings",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TotalDays",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CityId",
                table: "Bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnCityId",
                table: "Bookings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CityId",
                table: "Bookings",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ReturnCityId",
                table: "Bookings",
                column: "ReturnCityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Cities_CityId",
                table: "Bookings",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Cities_ReturnCityId",
                table: "Bookings",
                column: "ReturnCityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Cities_CityId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Cities_ReturnCityId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CityId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ReturnCityId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ReturnCityId",
                table: "Bookings");

            migrationBuilder.AlterColumn<double>(
                name: "TotalPrice",
                table: "Bookings",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "TotalDays",
                table: "Bookings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
