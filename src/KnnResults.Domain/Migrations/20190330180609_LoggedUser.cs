using Microsoft.EntityFrameworkCore.Migrations;

namespace KnnResults.Domain.Migrations
{
    public partial class LoggedUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "VisualAttributeDefinition",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "AttributeRejections",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User",
                table: "VisualAttributeDefinition");

            migrationBuilder.DropColumn(
                name: "User",
                table: "AttributeRejections");
        }
    }
}
