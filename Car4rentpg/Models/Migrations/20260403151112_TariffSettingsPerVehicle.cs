using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class TariffSettingsPerVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TariffSettings_Type",
                table: "TariffSettings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TariffSettings",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "VehicleId",
                table: "TariffSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TariffSettings_VehicleId_Type",
                table: "TariffSettings",
                columns: new[] { "VehicleId", "Type" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TariffSettings_Vehicles_VehicleId",
                table: "TariffSettings",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TariffSettings_Vehicles_VehicleId",
                table: "TariffSettings");

            migrationBuilder.DropIndex(
                name: "IX_TariffSettings_VehicleId_Type",
                table: "TariffSettings");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "TariffSettings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TariffSettings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.CreateIndex(
                name: "IX_TariffSettings_Type",
                table: "TariffSettings",
                column: "Type",
                unique: true);
        }
    }
}
