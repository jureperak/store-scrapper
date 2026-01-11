using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StoreScrapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adapters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adapters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    adapter_id = table.Column<int>(type: "integer", nullable: false),
                    product_page_url = table.Column<string>(type: "text", nullable: false),
                    availability_url = table.Column<string>(type: "text", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    check_interval_seconds = table.Column<int>(type: "integer", nullable: false),
                    hangfire_job_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_adapters_adapter_id",
                        column: x => x.adapter_id,
                        principalTable: "adapters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    product_sku_id = table.Column<int>(type: "integer", nullable: false),
                    product_page_url = table.Column<string>(type: "text", nullable: false),
                    email_sent = table.Column<bool>(type: "boolean", nullable: false),
                    email_body = table.Column<string>(type: "text", nullable: true),
                    whats_app_sent = table.Column<bool>(type: "boolean", nullable: false),
                    whats_app_body = table.Column<string>(type: "text", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_history_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_execution_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    notification_history_id = table.Column<int>(type: "integer", nullable: true),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    success = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_execution_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_job_execution_logs_notification_history_notification_histor",
                        column: x => x.notification_history_id,
                        principalTable: "notification_history",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_job_execution_logs_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_skus",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sku = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    job_execution_log_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_skus", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_skus_job_execution_logs_job_execution_log_id",
                        column: x => x.job_execution_log_id,
                        principalTable: "job_execution_logs",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_product_skus_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_history_product_sku",
                columns: table => new
                {
                    notification_histories_id = table.Column<int>(type: "integer", nullable: false),
                    product_skus_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_history_product_sku", x => new { x.notification_histories_id, x.product_skus_id });
                    table.ForeignKey(
                        name: "fk_notification_history_product_sku_notification_history_notif",
                        column: x => x.notification_histories_id,
                        principalTable: "notification_history",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notification_history_product_sku_product_skus_product_skus_",
                        column: x => x.product_skus_id,
                        principalTable: "product_skus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_job_execution_logs_notification_history_id",
                table: "job_execution_logs",
                column: "notification_history_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_execution_logs_product_id",
                table: "job_execution_logs",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_history_product_id",
                table: "notification_history",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_history_product_sku_product_skus_id",
                table: "notification_history_product_sku",
                column: "product_skus_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_skus_job_execution_log_id",
                table: "product_skus",
                column: "job_execution_log_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_skus_product_id",
                table: "product_skus",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_adapter_id",
                table: "products",
                column: "adapter_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_history_product_sku");

            migrationBuilder.DropTable(
                name: "product_skus");

            migrationBuilder.DropTable(
                name: "job_execution_logs");

            migrationBuilder.DropTable(
                name: "notification_history");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "adapters");
        }
    }
}
