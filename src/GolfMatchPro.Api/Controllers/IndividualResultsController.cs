using GolfMatchPro.Data;
using GolfMatchPro.Engine.Individual;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/bets/{betConfigId:int}/individual-results")]
public class IndividualResultsController(GolfMatchDbContext db, IIndividualBetCalculator calculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IndividualBetResultsDto>> GetResults(int matchId, int betConfigId)
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

        // Build player data from match scores
        var players = new List<IndividualPlayerData>();
        foreach (var score in match.Scores)
        {
            players.Add(new IndividualPlayerData
            {
                PlayerId = score.PlayerId,
                PlayerName = score.Player.FullName,
                CourseHandicap = score.CourseHandicap,
                GrossScores = score.GetHoleScores(),
            });
        }

        // Build matchups from teams: each team has 2 players (PlayerA vs PlayerB)
        // Individual bets store matchups as teams with 2 players each
        var matchups = new List<IndividualMatchup>();
        foreach (var team in config.Teams.OrderBy(t => t.TeamNumber))
        {
            var teamPlayers = team.Players.OrderBy(p => p.Position).ToList();
            if (teamPlayers.Count >= 2)
            {
                matchups.Add(new IndividualMatchup
                {
                    PlayerAId = teamPlayers[0].PlayerId,
                    PlayerBId = teamPlayers[1].PlayerId,
                });
            }
        }

        // If no teams defined, check ConfigJson for matchup pairs
        if (matchups.Count == 0 && !string.IsNullOrEmpty(config.ConfigJson))
        {
            try
            {
                var parsed = System.Text.Json.JsonSerializer.Deserialize<IndividualMatchupConfig>(
                    config.ConfigJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (parsed?.Matchups != null)
                {
                    matchups = parsed.Matchups.Select(m => new IndividualMatchup
                    {
                        PlayerAId = m.PlayerAId,
                        PlayerBId = m.PlayerBId,
                    }).ToList();
                }
            }
            catch
            {
                // Invalid JSON, proceed with empty matchups
            }
        }

        var betConfig = new IndividualBetConfig
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

        // Only include players that appear in matchups
        var matchupPlayerIds = matchups
            .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
            .ToHashSet();
        var relevantPlayers = players.Where(p => matchupPlayerIds.Contains(p.PlayerId)).ToList();

        var results = calculator.Calculate(betConfig, relevantPlayers, matchups);

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
                PressResult = pr.WinLoss - pr.WinLossAfterExpense,
                ResultDetailsJson = System.Text.Json.JsonSerializer.Serialize(pr),
            });
        }

        await db.SaveChangesAsync();
        return Ok();
    }

    private static IndividualBetResultsDto MapToDto(IndividualBetResults r) => new()
    {
        Matchups = r.Matchups.Select(m => new IndividualMatchupResultDto
        {
            PlayerAId = m.PlayerAId,
            PlayerAName = m.PlayerAName,
            PlayerBId = m.PlayerBId,
            PlayerBName = m.PlayerBName,
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
            TotalAmountPlayerA = m.TotalAmountPlayerA,
        }).ToList(),
        PlayerResults = r.PlayerResults.Select(p => new IndividualPlayerResultDto
        {
            PlayerId = p.PlayerId,
            PlayerName = p.PlayerName,
            WinLoss = p.WinLoss,
            WinLossAfterExpense = p.WinLossAfterExpense,
        }).ToList(),
    };
}

// DTOs
public class IndividualBetResultsDto
{
    public List<IndividualMatchupResultDto> Matchups { get; set; } = [];
    public List<IndividualPlayerResultDto> PlayerResults { get; set; } = [];
}

public class IndividualMatchupResultDto
{
    public int PlayerAId { get; set; }
    public string PlayerAName { get; set; } = string.Empty;
    public int PlayerBId { get; set; }
    public string PlayerBName { get; set; } = string.Empty;
    public int[] HoleByHoleStatus { get; set; } = [];
    public int Front9Result { get; set; }
    public int Back9Result { get; set; }
    public int Overall18Result { get; set; }
    public decimal NassauFrontDollars { get; set; }
    public decimal NassauBackDollars { get; set; }
    public decimal Nassau18Dollars { get; set; }
    public List<PressResultDto> Presses { get; set; } = [];
    public decimal TotalPressAmount { get; set; }
    public decimal TotalAmountPlayerA { get; set; }
}

public class PressResultDto
{
    public int StartHole { get; set; }
    public int EndHole { get; set; }
    public int Result { get; set; }
    public decimal Amount { get; set; }
}

public class IndividualPlayerResultDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public decimal WinLoss { get; set; }
    public decimal WinLossAfterExpense { get; set; }
}

// Config model for JSON parsing
public class IndividualMatchupConfig
{
    public List<IndividualMatchupJson> Matchups { get; set; } = [];
}

public class IndividualMatchupJson
{
    public int PlayerAId { get; set; }
    public int PlayerBId { get; set; }
}
