using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreScrapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "products",
                type: "text",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                table: "products");
        }
    }
}
