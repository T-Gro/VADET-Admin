using Microsoft.EntityFrameworkCore.Migrations;

namespace KnnResults.Domain.Migrations
{
    public partial class CascadedDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVisualAttributes_VisualAttributeDefinition",
                table: "ProductVisualAttributes");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVisualAttributes_VisualAttributeDefinition",
                table: "ProductVisualAttributes",
                column: "AttributeId",
                principalTable: "VisualAttributeDefinition",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVisualAttributes_VisualAttributeDefinition",
                table: "ProductVisualAttributes");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVisualAttributes_VisualAttributeDefinition",
                table: "ProductVisualAttributes",
                column: "AttributeId",
                principalTable: "VisualAttributeDefinition",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
