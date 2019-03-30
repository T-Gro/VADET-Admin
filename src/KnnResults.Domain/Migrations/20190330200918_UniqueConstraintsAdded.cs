using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KnnResults.Domain.Migrations
{
    public partial class UniqueConstraintsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVisualAttributes_ProductId",
                table: "ProductVisualAttributes");

            migrationBuilder.AlterColumn<string>(
                name: "AttributeSource",
                table: "VisualAttributeDefinition",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttributeSource",
                table: "AttributeRejections",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_VisualAttributeDefinition_Name",
                table: "VisualAttributeDefinition",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisualAttributeDefinition_AttributeSource_OriginalProposalId",
                table: "VisualAttributeDefinition",
                columns: new[] { "AttributeSource", "OriginalProposalId" },
                unique: true,
                filter: "[AttributeSource] IS NOT NULL")
                .Annotation("SqlServer:Include", new[] { "CreatedAt", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVisualAttributes_ProductId_AttributeId",
                table: "ProductVisualAttributes",
                columns: new[] { "ProductId", "AttributeId" },
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_AttributeRejections_AttributeSource_OriginalProposalId",
                table: "AttributeRejections",
                columns: new[] { "AttributeSource", "OriginalProposalId" },
                unique: true)
                .Annotation("SqlServer:Include", new[] { "Reason", "Time" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VisualAttributeDefinition_Name",
                table: "VisualAttributeDefinition");

            migrationBuilder.DropIndex(
                name: "IX_VisualAttributeDefinition_AttributeSource_OriginalProposalId",
                table: "VisualAttributeDefinition");

            migrationBuilder.DropIndex(
                name: "IX_ProductVisualAttributes_ProductId_AttributeId",
                table: "ProductVisualAttributes");

            migrationBuilder.DropIndex(
                name: "IX_AttributeRejections_AttributeSource_OriginalProposalId",
                table: "AttributeRejections");

            migrationBuilder.AlterColumn<string>(
                name: "AttributeSource",
                table: "VisualAttributeDefinition",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttributeSource",
                table: "AttributeRejections",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_ProductVisualAttributes_ProductId",
                table: "ProductVisualAttributes",
                column: "ProductId");
        }
    }
}
