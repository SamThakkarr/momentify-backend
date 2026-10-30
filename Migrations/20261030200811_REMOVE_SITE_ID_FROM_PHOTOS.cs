using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace momentify_backend.Migrations
{
    /// <inheritdoc />
    public partial class REMOVE_SITE_ID_FROM_PHOTOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Sites_SiteId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_SiteId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "Photos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                table: "Photos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_SiteId",
                table: "Photos",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Sites_SiteId",
                table: "Photos",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
