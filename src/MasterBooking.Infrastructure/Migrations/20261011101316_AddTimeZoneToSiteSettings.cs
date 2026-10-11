using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeZoneToSiteSettings : Migration
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "SiteSettings");
        }
    }
}
