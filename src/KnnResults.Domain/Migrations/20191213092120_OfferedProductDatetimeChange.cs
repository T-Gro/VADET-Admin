using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KnnResults.Domain.Migrations
{
    public partial class OfferedProductDatetimeChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "OfferedAttributeReaction",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "OfferedAttributeReaction",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");
        }
    }
}
