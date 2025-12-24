using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenCharge.Migrations
{
    public partial class DakikaSistemi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DurationHours",
                table: "Reservations",
                newName: "DurationMinutes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DurationMinutes",
                table: "Reservations",
                newName: "DurationHours");
        }
    }
}
