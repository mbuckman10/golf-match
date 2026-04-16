using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfMatchPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchNameToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchName",
                table: "Matches",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchName",
                table: "Matches");
        }
    }
}
