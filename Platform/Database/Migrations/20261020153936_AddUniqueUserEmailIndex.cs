using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace momentify_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueUserEmailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OurStoryData_Sites_SiteId1",
                table: "OurStoryData");

            migrationBuilder.DropIndex(
                name: "IX_OurStoryData_SiteId1",
                table: "OurStoryData");

            migrationBuilder.DropColumn(
                name: "SiteId1",
                table: "OurStoryData");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "SiteId1",
                table: "OurStoryData",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OurStoryData_SiteId1",
                table: "OurStoryData",
                column: "SiteId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OurStoryData_Sites_SiteId1",
                table: "OurStoryData",
                column: "SiteId1",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
