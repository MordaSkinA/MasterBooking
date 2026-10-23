using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterBooking.Infrastructure.Migrations
{

    public partial class AddTimeZoneToSiteSettings : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "SiteSettings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Asia/Vladivostok");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "SiteSettings");
        }
    }
}
