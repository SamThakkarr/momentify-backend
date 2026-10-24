using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace momentify_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteIdToPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                table: "Photos",
                type: "integer",
                nullable: true);

            // Existing photos belong to Site 1.
            migrationBuilder.Sql("""
                                 UPDATE "Photos"
                                 SET "SiteId" = 1
                                 WHERE "SiteId" IS NULL;
                                 """);

            migrationBuilder.AlterColumn<int>(
                name: "SiteId",
                table: "Photos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

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
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Sites_SiteId",
                table: "Photos");

            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Users_UserId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_SiteId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_UserId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Photos");
        }
    }
}
