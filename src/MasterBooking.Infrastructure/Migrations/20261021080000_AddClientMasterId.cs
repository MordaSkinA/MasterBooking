using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterBooking.Infrastructure.Migrations
{

    public partial class AddClientMasterId : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MasterId",
                table: "Clients",
                type: "text",
                nullable: false,
                defaultValue: "");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "Clients");
        }
    }
}
