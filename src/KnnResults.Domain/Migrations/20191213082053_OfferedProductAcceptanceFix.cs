using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KnnResults.Domain.Migrations
{
    public partial class OfferedProductAcceptanceFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OfferedAttributeReaction",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AttributeId = table.Column<int>(nullable: false),
                    ImageId = table.Column<string>(maxLength: 50, nullable: false),
                    User = table.Column<string>(maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    DistanceToAttribute = table.Column<double>(nullable: false),
                    ReactionStatus = table.Column<string>(maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferedAttributeReaction", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfferedAttributeReaction_ImageId_AttributeId",
                table: "OfferedAttributeReaction",
                columns: new[] { "ImageId", "AttributeId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfferedAttributeReaction");
        }
    }
}
