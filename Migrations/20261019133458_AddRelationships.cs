using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace momentify_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sites_TemplateId",
                table: "Sites",
                column: "TemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_Templates_TemplateId",
                table: "Sites",
                column: "TemplateId",
                principalTable: "Templates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_Templates_TemplateId",
                table: "Sites");

            migrationBuilder.DropIndex(
                name: "IX_Sites_TemplateId",
                table: "Sites");
        }
    }
}
