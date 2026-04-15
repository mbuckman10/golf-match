using GolfMatchPro.Data;
using GolfMatchPro.Engine.BestBall;
using GolfMatchPro.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/bets/{betConfigId:int}/bestball-results")]
public class BestBallResultsController(GolfMatchDbContext db, IBestBallCalculator calculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BestBallResultsDto>> GetResults(int matchId, int betConfigId)
    {
        var config = await db.BetConfigurations
            .Include(b => b.Teams)
                .ThenInclude(t => t.Players)
                    .ThenInclude(tp => tp.Player)
            .FirstOrDefaultAsync(b => b.BetConfigId == betConfigId && b.MatchId == matchId);

        if (config is null) return NotFound();

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

        // Team 1 is the Sheet Hanger team, remaining are opponents
        var orderedTeams = config.Teams.OrderBy(t => t.TeamNumber).ToList();
        if (orderedTeams.Count < 2)
            return BadRequest("Best Ball requires at least 2 teams (sheet hangers + 1 opponent).");

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

        return MapToDto(results);
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveResults(int matchId, int betConfigId)
    {
        var resultAction = await GetResults(matchId, betConfigId);
        if (resultAction.Result is not null)
            return resultAction.Result;

        var dto = resultAction.Value!;

        var oldResults = await db.BetResults
            .Where(r => r.BetConfigId == betConfigId)
            .ToListAsync();
        db.BetResults.RemoveRange(oldResults);

        foreach (var pr in dto.PlayerResults)
        {
            db.BetResults.Add(new Data.Entities.BetResult
            {
                BetConfigId = betConfigId,
                PlayerId = pr.PlayerId,
                WinLossAmount = pr.WinLossAfterExpense,
                ResultDetailsJson = System.Text.Json.JsonSerializer.Serialize(pr),
            });
        }

        await db.SaveChangesAsync();
        return Ok();
    }

    private static BestBallResultsDto MapToDto(BestBallResults r) => new()
    {
        Matchups = r.Matchups.Select(m => new BestBallMatchupResultDto
        {
            SheetHangerTeamNumber = m.SheetHangerTeamNumber,
            SheetHangerTeamName = m.SheetHangerTeamName,
            OpponentTeamNumber = m.OpponentTeamNumber,
            OpponentTeamName = m.OpponentTeamName,
            SheetHangerBestBall = m.SheetHangerBestBall,
            OpponentBestBall = m.OpponentBestBall,
            HoleByHoleStatus = m.Nassau.HoleByHoleStatus,
            Front9Result = m.Nassau.Front9Result,
            Back9Result = m.Nassau.Back9Result,
            Overall18Result = m.Nassau.Overall18Result,
            NassauFrontDollars = m.NassauFrontDollars,
            NassauBackDollars = m.NassauBackDollars,
            Nassau18Dollars = m.Nassau18Dollars,
            Presses = m.Presses.Select(p => new PressResultDto
            {
                StartHole = p.StartHole,
                EndHole = p.EndHole,
                Result = p.Result,
                Amount = p.Amount,
            }).ToList(),
            TotalPressAmount = m.TotalPressAmount,
            TotalAmountSheetHanger = m.TotalAmountSheetHanger,
        }).ToList(),
        PlayerResults = r.PlayerResults.Select(p => new BestBallPlayerResultDto
        {
            PlayerId = p.PlayerId,
            PlayerName = p.PlayerName,
            TeamNumber = p.TeamNumber,
            WinLoss = p.WinLoss,
            WinLossAfterExpense = p.WinLossAfterExpense,
        }).ToList(),
    };
}

// DTOs
public class BestBallResultsDto
{
    public List<BestBallMatchupResultDto> Matchups { get; set; } = [];
    public List<BestBallPlayerResultDto> PlayerResults { get; set; } = [];
}

public class BestBallMatchupResultDto
{
    public int SheetHangerTeamNumber { get; set; }
    public string? SheetHangerTeamName { get; set; }
    public int OpponentTeamNumber { get; set; }
    public string? OpponentTeamName { get; set; }
    public int[] SheetHangerBestBall { get; set; } = [];
    public int[] OpponentBestBall { get; set; } = [];
    public int[] HoleByHoleStatus { get; set; } = [];
    public int Front9Result { get; set; }
    public int Back9Result { get; set; }
    public int Overall18Result { get; set; }
    public decimal NassauFrontDollars { get; set; }
    public decimal NassauBackDollars { get; set; }
    public decimal Nassau18Dollars { get; set; }
    public List<PressResultDto> Presses { get; set; } = [];
    public decimal TotalPressAmount { get; set; }
    public decimal TotalAmountSheetHanger { get; set; }
}

public class BestBallPlayerResultDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int TeamNumber { get; set; }
    public decimal WinLoss { get; set; }
    public decimal WinLossAfterExpense { get; set; }
}
