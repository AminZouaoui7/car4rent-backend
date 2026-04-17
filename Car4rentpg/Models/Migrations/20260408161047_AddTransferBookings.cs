using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Car4rentpg.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferBookings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PickupAirportId = table.Column<string>(type: "text", nullable: false),
                    DropoffCityId = table.Column<string>(type: "text", nullable: false),
                    HotelName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    HotelAddress = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FlightNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Passengers = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferBookings_Cities_DropoffCityId",
                        column: x => x.DropoffCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransferBookings_Cities_PickupAirportId",
                        column: x => x.PickupAirportId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransferBookings_CreatedAt",
                table: "TransferBookings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransferBookings_DropoffCityId",
                table: "TransferBookings",
                column: "DropoffCityId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferBookings_PickupAirportId",
                table: "TransferBookings",
                column: "PickupAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferBookings_Status",
                table: "TransferBookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TransferBookings_TransferDate",
                table: "TransferBookings",
                column: "TransferDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferBookings");
        }
    }
}
