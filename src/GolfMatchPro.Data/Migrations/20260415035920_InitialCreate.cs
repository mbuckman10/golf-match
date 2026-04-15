using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfMatchPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeeColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    YearOfInfo = table.Column<int>(type: "int", nullable: true),
                    CourseRating = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SlopeRating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseId);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HandicapIndex = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsGuest = table.Column<bool>(type: "bit", nullable: false),
                    EntraUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "CourseHoles",
                columns: table => new
                {
                    CourseHoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    HoleNumber = table.Column<int>(type: "int", nullable: false),
                    Par = table.Column<int>(type: "int", nullable: false),
                    HandicapRanking = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseHoles", x => x.CourseHoleId);
                    table.CheckConstraint("CK_CourseHole_HandicapRanking", "[HandicapRanking] >= 1 AND [HandicapRanking] <= 18");
                    table.CheckConstraint("CK_CourseHole_HoleNumber", "[HoleNumber] >= 1 AND [HoleNumber] <= 18");
                    table.CheckConstraint("CK_CourseHole_Par", "[Par] >= 3 AND [Par] <= 6");
                    table.ForeignKey(
                        name: "FK_CourseHoles_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    MatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    MatchDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByPlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_Matches_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Players_CreatedByPlayerId",
                        column: x => x.CreatedByPlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BetConfigurations",
                columns: table => new
                {
                    BetConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    BetType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CompetitionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HandicapPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    NassauFront = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    NassauBack = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Nassau18 = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalStrokesBetPerStroke = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaxNetScore = table.Column<int>(type: "int", nullable: true),
                    InvestmentOffEnabled = table.Column<bool>(type: "bit", nullable: false),
                    InvestmentOffAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    RedemptionEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RedemptionAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DunnEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DunnAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AutoPressEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PressAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PressDownThreshold = table.Column<int>(type: "int", nullable: false),
                    SkinsBuyIn = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    SkinsPerSkinAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ExpenseDeductionPct = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ScoresCountingPerHole = table.Column<int>(type: "int", nullable: false),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetConfigurations", x => x.BetConfigId);
                    table.ForeignKey(
                        name: "FK_BetConfigurations_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchScores",
                columns: table => new
                {
                    MatchScoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    CourseHandicap = table.Column<int>(type: "int", nullable: false),
                    Hole1 = table.Column<int>(type: "int", nullable: false),
                    Hole2 = table.Column<int>(type: "int", nullable: false),
                    Hole3 = table.Column<int>(type: "int", nullable: false),
                    Hole4 = table.Column<int>(type: "int", nullable: false),
                    Hole5 = table.Column<int>(type: "int", nullable: false),
                    Hole6 = table.Column<int>(type: "int", nullable: false),
                    Hole7 = table.Column<int>(type: "int", nullable: false),
                    Hole8 = table.Column<int>(type: "int", nullable: false),
                    Hole9 = table.Column<int>(type: "int", nullable: false),
                    Hole10 = table.Column<int>(type: "int", nullable: false),
                    Hole11 = table.Column<int>(type: "int", nullable: false),
                    Hole12 = table.Column<int>(type: "int", nullable: false),
                    Hole13 = table.Column<int>(type: "int", nullable: false),
                    Hole14 = table.Column<int>(type: "int", nullable: false),
                    Hole15 = table.Column<int>(type: "int", nullable: false),
                    Hole16 = table.Column<int>(type: "int", nullable: false),
                    Hole17 = table.Column<int>(type: "int", nullable: false),
                    Hole18 = table.Column<int>(type: "int", nullable: false),
                    GrossTotal = table.Column<int>(type: "int", nullable: false),
                    NetTotal = table.Column<int>(type: "int", nullable: false),
                    ReportableScore = table.Column<int>(type: "int", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchScores", x => x.MatchScoreId);
                    table.ForeignKey(
                        name: "FK_MatchScores_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchScores_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BetResults",
                columns: table => new
                {
                    BetResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BetConfigId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    WinLossAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    NassauFrontResult = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    NassauBackResult = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Nassau18Result = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    InvestmentResult = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalStrokesResult = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SkinsWon = table.Column<int>(type: "int", nullable: true),
                    SkinsAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    PressResult = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    ResultDetailsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetResults", x => x.BetResultId);
                    table.ForeignKey(
                        name: "FK_BetResults_BetConfigurations_BetConfigId",
                        column: x => x.BetConfigId,
                        principalTable: "BetConfigurations",
                        principalColumn: "BetConfigId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BetResults_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BetConfigId = table.Column<int>(type: "int", nullable: false),
                    TeamNumber = table.Column<int>(type: "int", nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_Teams_BetConfigurations_BetConfigId",
                        column: x => x.BetConfigId,
                        principalTable: "BetConfigurations",
                        principalColumn: "BetConfigId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamPlayers",
                columns: table => new
                {
                    TeamPlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamPlayers", x => x.TeamPlayerId);
                    table.ForeignKey(
                        name: "FK_TeamPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamPlayers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BetConfigurations_MatchId",
                table: "BetConfigurations",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_BetResults_BetConfigId",
                table: "BetResults",
                column: "BetConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_BetResults_PlayerId",
                table: "BetResults",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseHoles_CourseId_HoleNumber",
                table: "CourseHoles",
                columns: new[] { "CourseId", "HoleNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CourseId",
                table: "Matches",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CreatedByPlayerId",
                table: "Matches",
                column: "CreatedByPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchScores_MatchId_PlayerId",
                table: "MatchScores",
                columns: new[] { "MatchId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchScores_PlayerId",
                table: "MatchScores",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPlayers_PlayerId",
                table: "TeamPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPlayers_TeamId_PlayerId",
                table: "TeamPlayers",
                columns: new[] { "TeamId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_BetConfigId",
                table: "Teams",
                column: "BetConfigId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BetResults");

            migrationBuilder.DropTable(
                name: "CourseHoles");

            migrationBuilder.DropTable(
                name: "MatchScores");

            migrationBuilder.DropTable(
                name: "TeamPlayers");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "BetConfigurations");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
