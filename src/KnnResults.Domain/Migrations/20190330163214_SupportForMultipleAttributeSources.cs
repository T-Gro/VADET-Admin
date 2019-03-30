using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KnnResults.Domain.Migrations
{
    public partial class SupportForMultipleAttributeSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VisualAttributeDefinition",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OriginalProposalId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Quality = table.Column<string>(maxLength: 50, nullable: true),
                    Candidates = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    DistanceTreshold = table.Column<double>(nullable: true),
                    DiscardedProducts = table.Column<string>(nullable: true),
                    DiscardedCategories = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualAttributeDefinition", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ZootBataProducts",
                columns: table => new
                {
                    ID = table.Column<string>(maxLength: 50, nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: false),
                    Brand = table.Column<string>(maxLength: 100, nullable: false),
                    Price = table.Column<int>(nullable: false),
                    Categories = table.Column<string>(nullable: false),
                    Tags = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZootBataProducts", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ProductVisualAttributes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<string>(maxLength: 50, nullable: false),
                    AttributeId = table.Column<int>(nullable: false),
                    Distance = table.Column<double>(nullable: false),
                    Coverage = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVisualAttributes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProductVisualAttributes_VisualAttributeDefinition",
                        column: x => x.AttributeId,
                        principalTable: "VisualAttributeDefinition",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductVisualAttributes_ZootBataProducts",
                        column: x => x.ProductId,
                        principalTable: "ZootBataProducts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVisualAttributes_AttributeId",
                table: "ProductVisualAttributes",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVisualAttributes_ProductId",
                table: "ProductVisualAttributes",
                column: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductVisualAttributes");

            migrationBuilder.DropTable(
                name: "VisualAttributeDefinition");

            migrationBuilder.DropTable(
                name: "ZootBataProducts");
        }
    }
}
