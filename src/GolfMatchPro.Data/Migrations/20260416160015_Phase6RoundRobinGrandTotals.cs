using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfMatchPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase6RoundRobinGrandTotals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrandTotals",
                columns: table => new
                {
                    GrandTotalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    IncludeFoursomes = table.Column<bool>(type: "bit", nullable: false),
                    IncludeThreesomes = table.Column<bool>(type: "bit", nullable: false),
                    IncludeFivesomes = table.Column<bool>(type: "bit", nullable: false),
                    IncludeIndividual = table.Column<bool>(type: "bit", nullable: false),
                    IncludeBestBall = table.Column<bool>(type: "bit", nullable: false),
                    IncludeSkinsGross = table.Column<bool>(type: "bit", nullable: false),
                    IncludeSkinsNet = table.Column<bool>(type: "bit", nullable: false),
                    IncludeIndoTourney = table.Column<bool>(type: "bit", nullable: false),
                    IncludeRoundRobins = table.Column<bool>(type: "bit", nullable: false),
                    TotalWinLoss = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DetailJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrandTotals", x => x.GrandTotalId);
                    table.ForeignKey(
                        name: "FK_GrandTotals_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GrandTotals_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoundRobinResults",
                columns: table => new
                {
                    RoundRobinResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    BetConfigId = table.Column<int>(type: "int", nullable: false),
                    RoundRobinType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MatchupsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeaderboardJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CalculatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoundRobinResults", x => x.RoundRobinResultId);
                    table.ForeignKey(
                        name: "FK_RoundRobinResults_BetConfigurations_BetConfigId",
                        column: x => x.BetConfigId,
                        principalTable: "BetConfigurations",
                        principalColumn: "BetConfigId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoundRobinResults_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrandTotals_MatchId_PlayerId",
                table: "GrandTotals",
                columns: new[] { "MatchId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrandTotals_PlayerId",
                table: "GrandTotals",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_RoundRobinResults_BetConfigId",
                table: "RoundRobinResults",
                column: "BetConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_RoundRobinResults_MatchId_BetConfigId_RoundRobinType",
                table: "RoundRobinResults",
                columns: new[] { "MatchId", "BetConfigId", "RoundRobinType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrandTotals");

            migrationBuilder.DropTable(
                name: "RoundRobinResults");
        }
    }
}
