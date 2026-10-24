using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace momentify_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUrlFromPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "Photos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Photos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
