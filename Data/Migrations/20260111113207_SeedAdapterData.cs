using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreScrapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdapterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "adapters",
                columns: new[] { "id", "created_at", "name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Zara" },
                    { 2, new DateTime(2026, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), "PullAndBear" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "adapters",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "adapters",
                keyColumn: "id",
                keyValue: 2);
        }
    }
}
