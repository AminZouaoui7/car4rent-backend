using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class AddLongTermRentalRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LongTermRentalRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationMonths = table.Column<int>(type: "integer", nullable: false),
                    PickupCityId = table.Column<string>(type: "text", nullable: false),
                    VehicleId = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    ProposedMonthlyPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ProposedTotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsQuoteSent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LongTermRentalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LongTermRentalRequests_Cities_PickupCityId",
                        column: x => x.PickupCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LongTermRentalRequests_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LongTermRentalRequests_CreatedAt",
                table: "LongTermRentalRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LongTermRentalRequests_PickupCityId",
                table: "LongTermRentalRequests",
                column: "PickupCityId");

            migrationBuilder.CreateIndex(
                name: "IX_LongTermRentalRequests_Status",
                table: "LongTermRentalRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LongTermRentalRequests_VehicleId",
                table: "LongTermRentalRequests",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LongTermRentalRequests");
        }
    }
}
