using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreScrapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class JobExecutionManyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_skus_job_execution_logs_job_execution_log_id",
                table: "product_skus");

            migrationBuilder.DropIndex(
                name: "ix_product_skus_job_execution_log_id",
                table: "product_skus");

            migrationBuilder.DropColumn(
                name: "job_execution_log_id",
                table: "product_skus");

            migrationBuilder.CreateTable(
                name: "job_execution_log_product_sku",
                columns: table => new
                {
                    job_execution_logs_id = table.Column<int>(type: "integer", nullable: false),
                    product_skus_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_execution_log_product_sku", x => new { x.job_execution_logs_id, x.product_skus_id });
                    table.ForeignKey(
                        name: "fk_job_execution_log_product_sku_job_execution_logs_job_execut",
                        column: x => x.job_execution_logs_id,
                        principalTable: "job_execution_logs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_job_execution_log_product_sku_product_skus_product_skus_id",
                        column: x => x.product_skus_id,
                        principalTable: "product_skus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_job_execution_log_product_sku_product_skus_id",
                table: "job_execution_log_product_sku",
                column: "product_skus_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_execution_log_product_sku");

            migrationBuilder.AddColumn<int>(
                name: "job_execution_log_id",
                table: "product_skus",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_skus_job_execution_log_id",
                table: "product_skus",
                column: "job_execution_log_id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_skus_job_execution_logs_job_execution_log_id",
                table: "product_skus",
                column: "job_execution_log_id",
                principalTable: "job_execution_logs",
                principalColumn: "id");
        }
    }
}
