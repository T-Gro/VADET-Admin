using Microsoft.EntityFrameworkCore.Migrations;

namespace KnnResults.Domain.Migrations
{
    public partial class SupportForMultipleSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttributeSource",
                table: "VisualAttributeDefinition",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttributeSource",
                table: "VisualAttributeDefinition");
        }
    }
}
