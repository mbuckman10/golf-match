using GolfMatchPro.Data;
using GolfMatchPro.Engine.Skins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/bets/{betConfigId:int}/skins-results")]
public class SkinsResultsController(GolfMatchDbContext db, ISkinsCalculator calculator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SkinsResultsDto>> GetResults(int matchId, int betConfigId)
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

        var skinsConfig = new SkinsConfig
        {
            UseNetScores = settings.UseNetScores,
            HandicapPercentage = config.HandicapPercentage,
            HoleHandicapRankings = holeRankings,
            BuyInPerPlayer = config.SkinsBuyIn,
            AmountPerSkin = config.SkinsPerSkinAmount,
        };

        var players = match.Scores
            .Select(s => new SkinsPlayerData
            {
                PlayerId = s.PlayerId,
                PlayerName = s.Player.FullName,
                CourseHandicap = s.CourseHandicap,
                GrossScores = s.GetHoleScores(),
            })
            .ToList();

        var result = calculator.Calculate(skinsConfig, players);
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

        foreach (var p in dto.PlayerResults)
        {
            db.BetResults.Add(new Data.Entities.BetResult
            {
                BetConfigId = betConfigId,
                PlayerId = p.PlayerId,
                WinLossAmount = p.NetWinnings,
                SkinsWon = p.SkinsWon,
                SkinsAmount = p.GrossWinnings,
                ResultDetailsJson = System.Text.Json.JsonSerializer.Serialize(p),
            });
        }

        await db.SaveChangesAsync();
        return Ok();
    }

    private static SkinsResultsDto MapToDto(SkinsResult r) => new()
    {
        TotalSkinsAwarded = r.TotalSkinsAwarded,
        UnresolvedCarrySkins = r.UnresolvedCarrySkins,
        TotalPot = r.TotalPot,
        AmountPerAwardedSkin = r.AmountPerAwardedSkin,
        HoleResults = r.HoleResults.Select(h => new SkinsHoleResultDto
        {
            HoleNumber = h.HoleNumber,
            CarryIn = h.CarryIn,
            CarryOut = h.CarryOut,
            WinnerPlayerId = h.WinnerPlayerId,
            WinnerPlayerName = h.WinnerPlayerName,
            SkinsAwarded = h.SkinsAwarded,
            WinningScore = h.WinningScore,
            TiedPlayerIds = h.TiedPlayerIds,
        }).ToList(),
        PlayerResults = r.PlayerResults.Select(p => new SkinsPlayerResultDto
        {
            PlayerId = p.PlayerId,
            PlayerName = p.PlayerName,
            SkinsWon = p.SkinsWon,
            GrossWinnings = p.GrossWinnings,
            NetWinnings = p.NetWinnings,
        }).ToList(),
    };

    private static SkinsConfigJson ParseSettings(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new SkinsConfigJson();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<SkinsConfigJson>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new SkinsConfigJson();
        }
        catch
        {
            return new SkinsConfigJson();
        }
    }
}

public class SkinsConfigJson
{
    public bool UseNetScores { get; set; } = true;
}

public class SkinsResultsDto
{
    public int TotalSkinsAwarded { get; set; }
    public int UnresolvedCarrySkins { get; set; }
    public decimal TotalPot { get; set; }
    public decimal AmountPerAwardedSkin { get; set; }
    public List<SkinsHoleResultDto> HoleResults { get; set; } = [];
    public List<SkinsPlayerResultDto> PlayerResults { get; set; } = [];
}

public class SkinsHoleResultDto
{
    public int HoleNumber { get; set; }
    public int CarryIn { get; set; }
    public int CarryOut { get; set; }
    public int? WinnerPlayerId { get; set; }
    public string? WinnerPlayerName { get; set; }
    public int SkinsAwarded { get; set; }
    public int WinningScore { get; set; }
    public List<int> TiedPlayerIds { get; set; } = [];
}

public class SkinsPlayerResultDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int SkinsWon { get; set; }
    public decimal GrossWinnings { get; set; }
    public decimal NetWinnings { get; set; }
}
