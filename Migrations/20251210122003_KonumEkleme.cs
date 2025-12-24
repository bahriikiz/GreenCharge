using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenCharge.Migrations
{
    public partial class KonumEkleme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "Stations",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "Stations");
        }
    }
}
