using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightInformationApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    AirportID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.AirportID);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    FlightID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FlightNumber = table.Column<string>(type: "TEXT", maxLength: 7, nullable: true),
                    Airline = table.Column<string>(type: "TEXT", nullable: true),
                    DepartureAirportAirportID = table.Column<int>(type: "INTEGER", nullable: true),
                    ArrivalAirportAirportID = table.Column<int>(type: "INTEGER", nullable: true),
                    DepartureTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ArrivalTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.FlightID);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_ArrivalAirportAirportID",
                        column: x => x.ArrivalAirportAirportID,
                        principalTable: "Airports",
                        principalColumn: "AirportID");
                    table.ForeignKey(
                        name: "FK_Flights_Airports_DepartureAirportAirportID",
                        column: x => x.DepartureAirportAirportID,
                        principalTable: "Airports",
                        principalColumn: "AirportID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ArrivalAirportAirportID",
                table: "Flights",
                column: "ArrivalAirportAirportID");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DepartureAirportAirportID",
                table: "Flights",
                column: "DepartureAirportAirportID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Airports");
        }
    }
}
