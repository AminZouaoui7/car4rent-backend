using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePricingRuleWithTariffSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingRules");

            migrationBuilder.CreateTable(
                name: "TariffSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    PriceStart = table.Column<decimal>(type: "numeric", nullable: false),
                    Price3Days = table.Column<decimal>(type: "numeric", nullable: false),
                    Price4To6Days = table.Column<decimal>(type: "numeric", nullable: false),
                    Price7To15Days = table.Column<decimal>(type: "numeric", nullable: false),
                    Price16To29Days = table.Column<decimal>(type: "numeric", nullable: false),
                    Price1Month = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TariffSettings_Type",
                table: "TariffSettings",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TariffSettings");

            migrationBuilder.CreateTable(
                name: "PricingRules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<string>(type: "text", nullable: true),
                    SeasonId = table.Column<string>(type: "text", nullable: true),
                    VehicleId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxDays = table.Column<int>(type: "integer", nullable: true),
                    MinDays = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PricePerDay = table.Column<double>(type: "double precision", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingRules_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PricingRules_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PricingRules_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingRules_CategoryId",
                table: "PricingRules",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingRules_SeasonId",
                table: "PricingRules",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingRules_VehicleId",
                table: "PricingRules",
                column: "VehicleId");
        }
    }
}
