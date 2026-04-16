using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfMatchPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPhase6DeleteBehaviors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrandTotals_Matches_MatchId",
                table: "GrandTotals");

            migrationBuilder.DropForeignKey(
                name: "FK_RoundRobinResults_Matches_MatchId",
                table: "RoundRobinResults");

            migrationBuilder.AddForeignKey(
                name: "FK_GrandTotals_Matches_MatchId",
                table: "GrandTotals",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoundRobinResults_Matches_MatchId",
                table: "RoundRobinResults",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrandTotals_Matches_MatchId",
                table: "GrandTotals");

            migrationBuilder.DropForeignKey(
                name: "FK_RoundRobinResults_Matches_MatchId",
                table: "RoundRobinResults");

            migrationBuilder.AddForeignKey(
                name: "FK_GrandTotals_Matches_MatchId",
                table: "GrandTotals",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoundRobinResults_Matches_MatchId",
                table: "RoundRobinResults",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
