using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreScrapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReactivationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "product_sku_re_activation_id",
                table: "product_skus",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "temporary_disabled",
                table: "product_skus",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "temporary_disabled",
                table: "product_skus",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
            
            migrationBuilder.CreateTable(
                name: "product_sku_re_activations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    re_enable_url = table.Column<string>(type: "text", nullable: false),
                    re_enable_used = table.Column<bool>(type: "boolean", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_sku_re_activations", x => x.id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_skus_product_sku_re_activations_product_sku_re_acti",
                table: "product_skus");

            migrationBuilder.DropTable(
                name: "product_sku_re_activations");

            migrationBuilder.DropIndex(
                name: "ix_product_skus_product_sku_re_activation_id",
                table: "product_skus");

            migrationBuilder.DropColumn(
                name: "product_sku_re_activation_id",
                table: "product_skus");

            migrationBuilder.DropColumn(
                name: "temporary_disabled",
                table: "product_skus");
        }
    }
}
