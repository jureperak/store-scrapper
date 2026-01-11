using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreScrapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReactivationTableRenamedIsUsed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_skus_product_sku_re_activations_product_sku_re_acti",
                table: "product_skus");

            migrationBuilder.DropIndex(
                name: "ix_product_skus_product_sku_re_activation_id",
                table: "product_skus");

            migrationBuilder.DropColumn(
                name: "product_sku_re_activation_id",
                table: "product_skus");

            migrationBuilder.RenameColumn(
                name: "re_enable_used",
                table: "product_sku_re_activations",
                newName: "is_used");

            migrationBuilder.AddColumn<int>(
                name: "product_sku_id",
                table: "product_sku_re_activations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_product_sku_re_activations_product_sku_id",
                table: "product_sku_re_activations",
                column: "product_sku_id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_sku_re_activations_product_skus_product_sku_id",
                table: "product_sku_re_activations",
                column: "product_sku_id",
                principalTable: "product_skus",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_sku_re_activations_product_skus_product_sku_id",
                table: "product_sku_re_activations");

            migrationBuilder.DropIndex(
                name: "ix_product_sku_re_activations_product_sku_id",
                table: "product_sku_re_activations");

            migrationBuilder.DropColumn(
                name: "product_sku_id",
                table: "product_sku_re_activations");

            migrationBuilder.RenameColumn(
                name: "is_used",
                table: "product_sku_re_activations",
                newName: "re_enable_used");

            migrationBuilder.AddColumn<int>(
                name: "product_sku_re_activation_id",
                table: "product_skus",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_skus_product_sku_re_activation_id",
                table: "product_skus",
                column: "product_sku_re_activation_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_product_skus_product_sku_re_activations_product_sku_re_acti",
                table: "product_skus",
                column: "product_sku_re_activation_id",
                principalTable: "product_sku_re_activations",
                principalColumn: "id");
        }
    }
}
