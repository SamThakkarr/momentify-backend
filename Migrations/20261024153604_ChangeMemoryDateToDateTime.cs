using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace momentify_backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMemoryDateToDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TABLE "Memories"
                                 ALTER COLUMN "Date"
                                 TYPE date
                                 USING "Date"::date;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TABLE "Memories"
                                 ALTER COLUMN "Date"
                                 TYPE text
                                 USING "Date"::text;
                                 """);
        }
    }
}
