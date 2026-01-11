using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreScrapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedProductSkuFromNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "product_sku_id",
                table: "notification_history");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "product_sku_id",
                table: "notification_history",
                type: "integer",
                nullable: false);
        }
    }
}
