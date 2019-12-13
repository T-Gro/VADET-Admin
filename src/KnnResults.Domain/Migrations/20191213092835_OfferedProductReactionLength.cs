using Microsoft.EntityFrameworkCore.Migrations;

namespace KnnResults.Domain.Migrations
{
    public partial class OfferedProductReactionLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReactionStatus",
                table: "OfferedAttributeReaction",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 10);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReactionStatus",
                table: "OfferedAttributeReaction",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 50);
        }
    }
}
