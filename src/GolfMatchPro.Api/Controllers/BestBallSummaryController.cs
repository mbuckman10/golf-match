using GolfMatchPro.Data;
using GolfMatchPro.Engine.BestBall;
using GolfMatchPro.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/bestball-summary")]
public class BestBallSummaryController(GolfMatchDbContext db, IBestBallCalculator calculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BestBallWinLossSummaryDto>> GetSummary(int matchId)
    {
        var match = await db.Matches
            .Include(m => m.Course)
                .ThenInclude(c => c.Holes)
            .Include(m => m.Scores)
                .ThenInclude(s => s.Player)
            .FirstOrDefaultAsync(m => m.MatchId == matchId);

        if (match is null) return NotFound();

        var holes = match.Course.Holes.OrderBy(h => h.HoleNumber).ToList();
        if (holes.Count != 18)
            return BadRequest("Course must have exactly 18 holes.");

        var holePars = holes.Select(h => h.Par).ToArray();
        var holeRankings = holes.Select(h => h.HandicapRanking).ToArray();

        // Find all BestBall bet configs for this match
        var bbConfigs = await db.BetConfigurations
            .Include(b => b.Teams)
                .ThenInclude(t => t.Players)
                    .ThenInclude(tp => tp.Player)
            .Where(b => b.MatchId == matchId && b.BetType == BetType.BestBall)
            .OrderBy(b => b.BetConfigId)
            .ToListAsync();

        if (bbConfigs.Count == 0)
            return Ok(new BestBallWinLossSummaryDto());

        var allResults = new List<BestBallResults>();

        foreach (var config in bbConfigs)
        {
            var orderedTeams = config.Teams.OrderBy(t => t.TeamNumber).ToList();
            if (orderedTeams.Count < 2) continue;

            BestBallTeamPair BuildTeamPair(Data.Entities.Team team)
            {
                var pair = new BestBallTeamPair
                {
                    TeamNumber = team.TeamNumber,
                    TeamName = team.TeamName,
                };
                foreach (var tp in team.Players)
                {
                    var score = match.Scores.FirstOrDefault(s => s.PlayerId == tp.PlayerId);
                    if (score is null) continue;
                    pair.Players.Add(new BestBallPlayerData
                    {
                        PlayerId = tp.PlayerId,
                        PlayerName = tp.Player?.FullName ?? string.Empty,
                        CourseHandicap = score.CourseHandicap,
                        GrossScores = score.GetHoleScores(),
                    });
                }
                return pair;
            }

            var sheetHangers = BuildTeamPair(orderedTeams[0]);
            var opponents = orderedTeams.Skip(1).Select(BuildTeamPair).ToList();

            var betConfig = new BestBallConfig
            {
                CompetitionType = config.CompetitionType,
                HandicapPercentage = config.HandicapPercentage,
                NassauFront = config.NassauFront,
                NassauBack = config.NassauBack,
                Nassau18 = config.Nassau18,
                AutoPressEnabled = config.AutoPressEnabled,
                PressAmount = config.PressAmount,
                PressDownThreshold = config.PressDownThreshold,
                ExpenseDeductionPct = config.ExpenseDeductionPct,
                HoleHandicapRankings = holeRankings,
                HolePars = holePars,
            };

            var results = calculator.Calculate(betConfig, sheetHangers, opponents);
            allResults.Add(results);
        }

        var summary = BestBallWinLossAggregator.Aggregate(allResults);

        return new BestBallWinLossSummaryDto
        {
            PlayerSummaries = summary.PlayerSummaries.Select(p => new BestBallPlayerSummaryDto
            {
                PlayerId = p.PlayerId,
                PlayerName = p.PlayerName,
                TotalWinLoss = p.TotalWinLoss,
                TotalWinLossAfterExpense = p.TotalWinLossAfterExpense,
                MatchupsPlayed = p.MatchupsPlayed,
                MatchupsWon = p.MatchupsWon,
                MatchupsLost = p.MatchupsLost,
                MatchupsTied = p.MatchupsTied,
            }).ToList()
        };
    }
}

public class BestBallWinLossSummaryDto
{
    public List<BestBallPlayerSummaryDto> PlayerSummaries { get; set; } = [];
}

public class BestBallPlayerSummaryDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public decimal TotalWinLoss { get; set; }
    public decimal TotalWinLossAfterExpense { get; set; }
    public int MatchupsPlayed { get; set; }
    public int MatchupsWon { get; set; }
    public int MatchupsLost { get; set; }
    public int MatchupsTied { get; set; }
}
