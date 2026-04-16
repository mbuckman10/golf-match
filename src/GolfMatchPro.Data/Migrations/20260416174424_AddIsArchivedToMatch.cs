using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfMatchPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Matches");
        }
    }
}
