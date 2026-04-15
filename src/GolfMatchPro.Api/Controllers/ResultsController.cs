using GolfMatchPro.Data;
using GolfMatchPro.Engine.Teams;
using GolfMatchPro.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/bets/{betConfigId:int}/results")]
public class ResultsController(GolfMatchDbContext db, ITeamBetCalculator calculator) : ControllerBase
{
    /// <summary>
    /// Computes live results for a bet configuration by running the engine.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<TeamBetResultsDto>> GetResults(int matchId, int betConfigId)
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

        // Build engine input
        var teams = new List<TeamData>();
        foreach (var team in config.Teams.OrderBy(t => t.TeamNumber))
        {
            var teamData = new TeamData
            {
                TeamNumber = team.TeamNumber,
                TeamName = team.TeamName,
            };

            foreach (var tp in team.Players)
            {
                var score = match.Scores.FirstOrDefault(s => s.PlayerId == tp.PlayerId);
                if (score is null) continue;

                teamData.Players.Add(new TeamPlayerData
                {
                    PlayerId = tp.PlayerId,
                    PlayerName = tp.Player?.FullName ?? string.Empty,
                    CourseHandicap = score.CourseHandicap,
                    GrossScores = score.GetHoleScores(),
                });
            }

            teams.Add(teamData);
        }

        var betConfig = new TeamBetConfig
        {
            CompetitionType = config.CompetitionType,
            HandicapPercentage = config.HandicapPercentage,
            ScoresCountingPerHole = config.ScoresCountingPerHole,
            NassauFront = config.NassauFront,
            NassauBack = config.NassauBack,
            Nassau18 = config.Nassau18,
            InvestmentOffEnabled = config.InvestmentOffEnabled,
            InvestmentOffAmount = config.InvestmentOffAmount,
            RedemptionEnabled = config.RedemptionEnabled,
            RedemptionAmount = config.RedemptionAmount,
            DunnEnabled = config.DunnEnabled,
            DunnAmount = config.DunnAmount,
            TotalStrokesBetPerStroke = config.TotalStrokesBetPerStroke,
            MaxNetScore = config.MaxNetScore,
            ExpenseDeductionPct = config.ExpenseDeductionPct,
            HoleHandicapRankings = holeRankings,
            HolePars = holePars,
        };

        var results = calculator.Calculate(betConfig, teams);

        return MapToDto(results);
    }

    /// <summary>
    /// Saves the computed results to the database as BetResult records.
    /// </summary>
    [HttpPost("save")]
    public async Task<IActionResult> SaveResults(int matchId, int betConfigId)
    {
        // First compute
        var resultAction = await GetResults(matchId, betConfigId);
        if (resultAction.Result is not null)
            return resultAction.Result; // NotFound or BadRequest

        var dto = resultAction.Value!;

        // Remove old results
        var oldResults = await db.BetResults
            .Where(r => r.BetConfigId == betConfigId)
            .ToListAsync();
        db.BetResults.RemoveRange(oldResults);

        // Insert new results
        foreach (var pr in dto.PlayerResults)
        {
            db.BetResults.Add(new Data.Entities.BetResult
            {
                BetConfigId = betConfigId,
                PlayerId = pr.PlayerId,
                WinLossAmount = pr.WinLossAfterExpense,
                NassauFrontResult = 0, // Detailed breakdown not needed for now
                NassauBackResult = 0,
                Nassau18Result = 0,
                InvestmentResult = 0,
                TotalStrokesResult = 0,
                ResultDetailsJson = System.Text.Json.JsonSerializer.Serialize(pr),
            });
        }

        await db.SaveChangesAsync();
        return Ok();
    }

    private static TeamBetResultsDto MapToDto(TeamBetResults r) => new()
    {
        TeamResults = r.TeamResults.Select(t => new TeamResultDto
        {
            TeamNumber = t.TeamNumber,
            TeamName = t.TeamName,
            TeamHoleScores = t.TeamHoleScores,
            IsOff = t.Investments.IsOff,
            IsRedemption = t.Investments.IsRedemption,
            TotalOffs = t.Investments.TotalOffs,
            TotalRedemptions = t.Investments.TotalRedemptions,
            InvestmentAmount = t.InvestmentAmount,
            NassauTotal = t.NassauTotal,
            TotalStrokesTotal = t.TotalStrokesTotal,
            GrandTotal = t.GrandTotal,
            GrandTotalAfterExpense = t.GrandTotalAfterExpense,
            TeamNetTotal = t.TeamNetTotal,
        }).ToList(),
        Matchups = r.Matchups.Select(m => new TeamVsTeamResultDto
        {
            TeamANumber = m.TeamANumber,
            TeamBNumber = m.TeamBNumber,
            NassauFrontDollars = m.NassauFrontDollars,
            NassauBackDollars = m.NassauBackDollars,
            Nassau18Dollars = m.Nassau18Dollars,
            TotalStrokesDollars = m.TotalStrokesDollars,
            HoleByHoleStatus = m.Nassau.HoleByHoleStatus,
            Front9Result = m.Nassau.Front9Result,
            Back9Result = m.Nassau.Back9Result,
            Overall18Result = m.Nassau.Overall18Result,
        }).ToList(),
        PlayerResults = r.PlayerResults.Select(p => new PlayerResultDto
        {
            PlayerId = p.PlayerId,
            PlayerName = p.PlayerName,
            TeamNumber = p.TeamNumber,
            WinLoss = p.WinLoss,
            WinLossAfterExpense = p.WinLossAfterExpense,
        }).ToList(),
    };
}

// DTOs for results endpoint
public class TeamBetResultsDto
{
    public List<TeamResultDto> TeamResults { get; set; } = [];
    public List<TeamVsTeamResultDto> Matchups { get; set; } = [];
    public List<PlayerResultDto> PlayerResults { get; set; } = [];
}

public class TeamResultDto
{
    public int TeamNumber { get; set; }
    public string? TeamName { get; set; }
    public int[] TeamHoleScores { get; set; } = [];
    public bool[] IsOff { get; set; } = [];
    public bool[] IsRedemption { get; set; } = [];
    public int TotalOffs { get; set; }
    public int TotalRedemptions { get; set; }
    public decimal InvestmentAmount { get; set; }
    public decimal NassauTotal { get; set; }
    public decimal TotalStrokesTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal GrandTotalAfterExpense { get; set; }
    public int TeamNetTotal { get; set; }
}

public class TeamVsTeamResultDto
{
    public int TeamANumber { get; set; }
    public int TeamBNumber { get; set; }
    public decimal NassauFrontDollars { get; set; }
    public decimal NassauBackDollars { get; set; }
    public decimal Nassau18Dollars { get; set; }
    public decimal TotalStrokesDollars { get; set; }
    public int[] HoleByHoleStatus { get; set; } = [];
    public int Front9Result { get; set; }
    public int Back9Result { get; set; }
    public int Overall18Result { get; set; }
}

public class PlayerResultDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int TeamNumber { get; set; }
    public decimal WinLoss { get; set; }
    public decimal WinLossAfterExpense { get; set; }
}
