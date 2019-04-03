using Microsoft.EntityFrameworkCore.Migrations;

namespace KnnResults.Domain.Migrations
{
    public partial class WhitelistedCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhitelistedCategories",
                table: "VisualAttributeDefinition",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhitelistedCategories",
                table: "VisualAttributeDefinition");
        }
    }
}
