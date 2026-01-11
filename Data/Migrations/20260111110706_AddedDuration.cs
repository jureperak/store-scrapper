using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreScrapper.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "duration",
                table: "job_execution_logs",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "duration",
                table: "job_execution_logs");
        }
    }
}
