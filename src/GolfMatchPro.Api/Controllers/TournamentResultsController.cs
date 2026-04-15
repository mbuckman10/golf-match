using GolfMatchPro.Data;
using GolfMatchPro.Engine.Tournament;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/bets/{betConfigId:int}/tournament-results")]
public class TournamentResultsController(GolfMatchDbContext db, ITournamentCalculator calculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TournamentResultsDto>> GetResults(int matchId, int betConfigId)
    {
        var config = await db.BetConfigurations
            .FirstOrDefaultAsync(b => b.BetConfigId == betConfigId && b.MatchId == matchId);

        if (config is null) return NotFound();

        var match = await db.Matches
            .Include(m => m.Course)
                .ThenInclude(c => c.Holes)
            .Include(m => m.Scores)
                .ThenInclude(s => s.Player)
            .FirstOrDefaultAsync(m => m.MatchId == matchId);

        if (match is null) return NotFound();

        var holeRankings = match.Course.Holes
            .OrderBy(h => h.HoleNumber)
            .Select(h => h.HandicapRanking)
            .ToArray();

        if (holeRankings.Length != 18)
            return BadRequest("Course must have exactly 18 holes.");

        var settings = ParseSettings(config.ConfigJson);

        var tournamentConfig = new TournamentConfig
        {
            SponsorMoney = settings.SponsorMoney,
            BuyInPerPlayer = settings.BuyInPerPlayer,
            ExpenseDeductionPct = config.ExpenseDeductionPct,
            HandicapPercentage = config.HandicapPercentage,
            HoleHandicapRankings = holeRankings,
            GrossPursePercent = settings.GrossPursePercent,
            NetPursePercent = settings.NetPursePercent,
            EighteenHolePercent = settings.EighteenHolePercent,
            FrontNinePercent = settings.FrontNinePercent,
            BackNinePercent = settings.BackNinePercent,
            PlacePayouts = settings.PlacePayouts.Select(p => new PlacePayout { Place = p.Place, Percent = p.Percent }).ToList(),
        };

        var players = match.Scores
            .Select(s => new TournamentPlayerData
            {
                PlayerId = s.PlayerId,
                PlayerName = s.Player.FullName,
                CourseHandicap = s.CourseHandicap,
                GrossScores = s.GetHoleScores(),
            })
            .ToList();

        var result = calculator.Calculate(tournamentConfig, players);
        return MapToDto(result);
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveResults(int matchId, int betConfigId)
    {
        var action = await GetResults(matchId, betConfigId);
        if (action.Result is not null)
            return action.Result;

        var dto = action.Value!;

        var oldResults = await db.BetResults
            .Where(r => r.BetConfigId == betConfigId)
            .ToListAsync();
        db.BetResults.RemoveRange(oldResults);

        foreach (var p in dto.Leaderboard)
        {
            db.BetResults.Add(new Data.Entities.BetResult
            {
                BetConfigId = betConfigId,
                PlayerId = p.PlayerId,
                WinLossAmount = p.TotalPayout,
                ResultDetailsJson = System.Text.Json.JsonSerializer.Serialize(p),
            });
        }

        await db.SaveChangesAsync();
        return Ok();
    }

    private static TournamentConfigJson ParseSettings(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new TournamentConfigJson();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<TournamentConfigJson>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new TournamentConfigJson();
        }
        catch
        {
            return new TournamentConfigJson();
        }
    }

    private static TournamentResultsDto MapToDto(TournamentResult r) => new()
    {
        PrizePool = r.PrizePool,
        GrossPurse = r.GrossPurse,
        NetPurse = r.NetPurse,
        Gross18 = MapDivision(r.Gross18),
        GrossFront9 = MapDivision(r.GrossFront9),
        GrossBack9 = MapDivision(r.GrossBack9),
        Net18 = MapDivision(r.Net18),
        NetFront9 = MapDivision(r.NetFront9),
        NetBack9 = MapDivision(r.NetBack9),
        Leaderboard = r.Leaderboard.Select(l => new TournamentLeaderboardEntryDto
        {
            PlayerId = l.PlayerId,
            PlayerName = l.PlayerName,
            Gross18 = l.Gross18,
            Net18 = l.Net18,
            GrossPayout = l.GrossPayout,
            NetPayout = l.NetPayout,
            TotalPayout = l.TotalPayout,
        }).ToList(),
    };

    private static TournamentDivisionResultDto MapDivision(TournamentDivisionResult d) => new()
    {
        Name = d.Name,
        Purse = d.Purse,
        Entries = d.Entries.Select(e => new TournamentDivisionEntryDto
        {
            PlayerId = e.PlayerId,
            PlayerName = e.PlayerName,
            Score = e.Score,
            Place = e.Place,
            Payout = e.Payout,
        }).ToList(),
    };
}

public class TournamentConfigJson
{
    public decimal SponsorMoney { get; set; }
    public decimal BuyInPerPlayer { get; set; } = 20m;
    public decimal GrossPursePercent { get; set; } = 50m;
    public decimal NetPursePercent { get; set; } = 50m;
    public decimal EighteenHolePercent { get; set; } = 60m;
    public decimal FrontNinePercent { get; set; } = 20m;
    public decimal BackNinePercent { get; set; } = 20m;
    public List<PlacePayoutDto> PlacePayouts { get; set; } = [];
}

public class PlacePayoutDto
{
    public int Place { get; set; }
    public decimal Percent { get; set; }
}

public class TournamentResultsDto
{
    public decimal PrizePool { get; set; }
    public decimal GrossPurse { get; set; }
    public decimal NetPurse { get; set; }
    public TournamentDivisionResultDto Gross18 { get; set; } = new();
    public TournamentDivisionResultDto GrossFront9 { get; set; } = new();
    public TournamentDivisionResultDto GrossBack9 { get; set; } = new();
    public TournamentDivisionResultDto Net18 { get; set; } = new();
    public TournamentDivisionResultDto NetFront9 { get; set; } = new();
    public TournamentDivisionResultDto NetBack9 { get; set; } = new();
    public List<TournamentLeaderboardEntryDto> Leaderboard { get; set; } = [];
}

public class TournamentDivisionResultDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Purse { get; set; }
    public List<TournamentDivisionEntryDto> Entries { get; set; } = [];
}

public class TournamentDivisionEntryDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Place { get; set; }
    public decimal Payout { get; set; }
}

public class TournamentLeaderboardEntryDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Gross18 { get; set; }
    public int Net18 { get; set; }
    public decimal GrossPayout { get; set; }
    public decimal NetPayout { get; set; }
    public decimal TotalPayout { get; set; }
}
