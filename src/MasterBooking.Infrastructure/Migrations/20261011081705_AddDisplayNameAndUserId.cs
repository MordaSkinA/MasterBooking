using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MasterBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayNameAndUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Masters_MasterId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Masters_MasterId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_DayOffs_Masters_MasterId",
                table: "DayOffs");

            migrationBuilder.DropForeignKey(
                name: "FK_GalleryImages_Masters_MasterId",
                table: "GalleryImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Masters_MasterId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_SiteSettings_Masters_MasterId",
                table: "SiteSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkingHours_Masters_MasterId",
                table: "WorkingHours");

            migrationBuilder.DropTable(
                name: "Masters");

            migrationBuilder.DropIndex(
                name: "IX_WorkingHours_MasterId",
                table: "WorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_SiteSettings_MasterId",
                table: "SiteSettings");

            migrationBuilder.DropIndex(
                name: "IX_Services_MasterId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_GalleryImages_MasterId",
                table: "GalleryImages");

            migrationBuilder.DropIndex(
                name: "IX_DayOffs_MasterId",
                table: "DayOffs");

            migrationBuilder.DropIndex(
                name: "IX_Clients_MasterId",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_MasterId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "WorkingHours");

            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "GalleryImages");

            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "DayOffs");

            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MasterId",
                table: "Appointments");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "SiteSettings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SiteSettings",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SiteSettings");

            migrationBuilder.AddColumn<int>(
                name: "MasterId",
                table: "WorkingHours",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MasterId",
                table: "SiteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MasterId",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MasterId",
                table: "GalleryImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MasterId",
                table: "DayOffs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MasterId",
                table: "Clients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MasterId",
                table: "Appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Masters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Masters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_MasterId",
                table: "WorkingHours",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteSettings_MasterId",
                table: "SiteSettings",
                column: "MasterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_MasterId",
                table: "Services",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryImages_MasterId",
                table: "GalleryImages",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOffs_MasterId",
                table: "DayOffs",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_MasterId",
                table: "Clients",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_MasterId",
                table: "Appointments",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_Masters_Slug",
                table: "Masters",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Masters_MasterId",
                table: "Appointments",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Masters_MasterId",
                table: "Clients",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DayOffs_Masters_MasterId",
                table: "DayOffs",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GalleryImages_Masters_MasterId",
                table: "GalleryImages",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Masters_MasterId",
                table: "Services",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSettings_Masters_MasterId",
                table: "SiteSettings",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkingHours_Masters_MasterId",
                table: "WorkingHours",
                column: "MasterId",
                principalTable: "Masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
