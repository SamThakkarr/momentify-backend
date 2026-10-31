using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace momentify_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotoId",
                table: "MemoryPhotos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CoverPhotoId",
                table: "OurStoryData",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemoryPhotoId",
                table: "OurStoryData",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LegacyMemoryPhotoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.CheckConstraint("CK_Photos_DisplayOrder", "\"DisplayOrder\" = -1 OR \"DisplayOrder\" >= 1");
                });

            // Preserve existing MemoryPhoto URLs by migrating each one into a
            // generic Photo row and linking the original association to it.
            migrationBuilder.Sql("""
                INSERT INTO "Photos"
                    ("StorageKey", "Url", "OriginalFileName", "ContentType", "DisplayOrder", "CreatedAt", "LegacyMemoryPhotoId")
                SELECT
                    'legacy/' || mp."Id"::text,
                    mp."ImageUrl",
                    'legacy-' || mp."Id"::text,
                    CASE
                        WHEN lower(mp."ImageUrl") ~ '\.png([?#].*)?$' THEN 'image/png'
                        WHEN lower(mp."ImageUrl") ~ '\.webp([?#].*)?$' THEN 'image/webp'
                        WHEN lower(mp."ImageUrl") ~ '\.gif([?#].*)?$' THEN 'image/gif'
                        WHEN lower(mp."ImageUrl") ~ '\.jpe?g([?#].*)?$' THEN 'image/jpeg'
                        ELSE 'application/octet-stream'
                    END,
                    CASE WHEN mp."DisplayOrder" >= 0 THEN mp."DisplayOrder" + 1 ELSE -1 END,
                    NOW(),
                    mp."Id"
                FROM "MemoryPhotos" mp;

                UPDATE "MemoryPhotos" mp
                SET "PhotoId" = p."Id"
                FROM "Photos" p
                WHERE p."LegacyMemoryPhotoId" = mp."Id";
                """);

            migrationBuilder.AlterColumn<int>(
                name: "PhotoId",
                table: "MemoryPhotos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MemoryPhotos");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "MemoryPhotos");

            migrationBuilder.DropColumn(
                name: "LegacyMemoryPhotoId",
                table: "Photos");

            migrationBuilder.CreateIndex(
                name: "IX_OurStoryData_CoverPhotoId",
                table: "OurStoryData",
                column: "CoverPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_OurStoryData_MemoryPhotoId",
                table: "OurStoryData",
                column: "MemoryPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_MemoryPhotos_PhotoId",
                table: "MemoryPhotos",
                column: "PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemoryPhotos_Photos_PhotoId",
                table: "MemoryPhotos",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemoryPhotos_Photos_PhotoId",
                table: "MemoryPhotos");

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

            migrationBuilder.DropIndex(
                name: "IX_MemoryPhotos_PhotoId",
                table: "MemoryPhotos");

            migrationBuilder.DropColumn(
                name: "CoverPhotoId",
                table: "OurStoryData");

            migrationBuilder.DropColumn(
                name: "MemoryPhotoId",
                table: "OurStoryData");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MemoryPhotos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "MemoryPhotos",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "MemoryPhotos" mp
                SET "ImageUrl" = p."Url", "DisplayOrder" = p."DisplayOrder"
                FROM "Photos" p
                WHERE p."Id" = mp."PhotoId";
                """);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "MemoryPhotos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DisplayOrder",
                table: "MemoryPhotos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "MemoryPhotos");

            migrationBuilder.DropTable(
                name: "Photos");
        }
    }
}
