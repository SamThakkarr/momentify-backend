using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace momentify_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTheBigAskTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OurStoryData_Photos_CoverPhotoId",
                table: "OurStoryData");

            migrationBuilder.DropForeignKey(
                name: "FK_OurStoryData_Photos_MemoryPhotoId",
                table: "OurStoryData");

            migrationBuilder.DropIndex(
                name: "IX_OurStoryData_CoverPhotoId",
                table: "OurStoryData");

            migrationBuilder.DropIndex(
                name: "IX_OurStoryData_MemoryPhotoId",
                table: "OurStoryData");

            migrationBuilder.CreateTable(
                name: "TheBigAskData",
                columns: table => new
                {
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PhotoMainPageId = table.Column<int>(type: "integer", nullable: false),
                    PhotoYesPageId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheBigAskData", x => x.SiteId);
                    table.ForeignKey(
                        name: "FK_TheBigAskData_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoButtonTexts",
                columns: table => new
                {
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoButtonTexts", x => new { x.SiteId, x.DisplayOrder });
                    table.ForeignKey(
                        name: "FK_NoButtonTexts_TheBigAskData_SiteId",
                        column: x => x.SiteId,
                        principalTable: "TheBigAskData",
                        principalColumn: "SiteId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoButtonTexts");

            migrationBuilder.DropTable(
                name: "TheBigAskData");

            migrationBuilder.CreateIndex(
                name: "IX_OurStoryData_CoverPhotoId",
                table: "OurStoryData",
                column: "CoverPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_OurStoryData_MemoryPhotoId",
                table: "OurStoryData",
                column: "MemoryPhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_OurStoryData_Photos_CoverPhotoId",
                table: "OurStoryData",
                column: "CoverPhotoId",
                principalTable: "Photos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OurStoryData_Photos_MemoryPhotoId",
                table: "OurStoryData",
                column: "MemoryPhotoId",
                principalTable: "Photos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
