using GolfMatchPro.Data;
using GolfMatchPro.Data.Entities;
using GolfMatchPro.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/bets")]
public class BetsController(GolfMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<BetConfigurationDto>>> GetAll(int matchId)
    {
        var match = await db.Matches.FindAsync(matchId);
        if (match is null) return NotFound();

        var configs = await db.BetConfigurations
            .Include(b => b.Teams)
                .ThenInclude(t => t.Players)
                    .ThenInclude(tp => tp.Player)
            .Where(b => b.MatchId == matchId)
            .OrderBy(b => b.BetConfigId)
            .ToListAsync();

        return configs.Select(MapToDto).ToList();
    }

    [HttpGet("{betConfigId:int}")]
    public async Task<ActionResult<BetConfigurationDto>> GetById(int matchId, int betConfigId)
    {
        var config = await db.BetConfigurations
            .Include(b => b.Teams)
                .ThenInclude(t => t.Players)
                    .ThenInclude(tp => tp.Player)
            .FirstOrDefaultAsync(b => b.BetConfigId == betConfigId && b.MatchId == matchId);

        if (config is null) return NotFound();
        return MapToDto(config);
    }

    [HttpPost]
    public async Task<ActionResult<BetConfigurationDto>> Create(int matchId, CreateBetConfigurationRequest request)
    {
        var match = await db.Matches.FindAsync(matchId);
        if (match is null) return NotFound();

        var config = new BetConfiguration
        {
            MatchId = matchId,
            BetType = request.BetType,
            CompetitionType = request.CompetitionType,
            HandicapPercentage = request.HandicapPercentage,
            NassauFront = request.NassauFront,
            NassauBack = request.NassauBack,
            Nassau18 = request.Nassau18,
            TotalStrokesBetPerStroke = request.TotalStrokesBetPerStroke,
            MaxNetScore = request.MaxNetScore,
            InvestmentOffEnabled = request.InvestmentOffEnabled,
            InvestmentOffAmount = request.InvestmentOffAmount,
            RedemptionEnabled = request.RedemptionEnabled,
            RedemptionAmount = request.RedemptionAmount,
            DunnEnabled = request.DunnEnabled,
            DunnAmount = request.DunnAmount,
            AutoPressEnabled = request.AutoPressEnabled,
            PressAmount = request.PressAmount,
            PressDownThreshold = request.PressDownThreshold,
            SkinsBuyIn = request.SkinsBuyIn,
            SkinsPerSkinAmount = request.SkinsPerSkinAmount,
            ExpenseDeductionPct = request.ExpenseDeductionPct,
            ScoresCountingPerHole = request.ScoresCountingPerHole,
            ConfigJson = request.ConfigJson,
        };

        db.BetConfigurations.Add(config);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { matchId, betConfigId = config.BetConfigId }, MapToDto(config));
    }

    [HttpPut("{betConfigId:int}")]
    public async Task<ActionResult<BetConfigurationDto>> Update(int matchId, int betConfigId, CreateBetConfigurationRequest request)
    {
        var config = await db.BetConfigurations
            .FirstOrDefaultAsync(b => b.BetConfigId == betConfigId && b.MatchId == matchId);

        if (config is null) return NotFound();

        config.BetType = request.BetType;
        config.CompetitionType = request.CompetitionType;
        config.HandicapPercentage = request.HandicapPercentage;
        config.NassauFront = request.NassauFront;
        config.NassauBack = request.NassauBack;
        config.Nassau18 = request.Nassau18;
        config.TotalStrokesBetPerStroke = request.TotalStrokesBetPerStroke;
        config.MaxNetScore = request.MaxNetScore;
        config.InvestmentOffEnabled = request.InvestmentOffEnabled;
        config.InvestmentOffAmount = request.InvestmentOffAmount;
        config.RedemptionEnabled = request.RedemptionEnabled;
        config.RedemptionAmount = request.RedemptionAmount;
        config.DunnEnabled = request.DunnEnabled;
        config.DunnAmount = request.DunnAmount;
        config.AutoPressEnabled = request.AutoPressEnabled;
        config.PressAmount = request.PressAmount;
        config.PressDownThreshold = request.PressDownThreshold;
        config.SkinsBuyIn = request.SkinsBuyIn;
        config.SkinsPerSkinAmount = request.SkinsPerSkinAmount;
        config.ExpenseDeductionPct = request.ExpenseDeductionPct;
        config.ScoresCountingPerHole = request.ScoresCountingPerHole;
        config.ConfigJson = request.ConfigJson;

        await db.SaveChangesAsync();

        return MapToDto(config);
    }

    [HttpDelete("{betConfigId:int}")]
    public async Task<IActionResult> Delete(int matchId, int betConfigId)
    {
        var config = await db.BetConfigurations
            .FirstOrDefaultAsync(b => b.BetConfigId == betConfigId && b.MatchId == matchId);

        if (config is null) return NotFound();

        db.BetConfigurations.Remove(config);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static BetConfigurationDto MapToDto(BetConfiguration b) => new()
    {
        BetConfigId = b.BetConfigId,
        MatchId = b.MatchId,
        BetType = b.BetType,
        CompetitionType = b.CompetitionType,
        HandicapPercentage = b.HandicapPercentage,
        NassauFront = b.NassauFront,
        NassauBack = b.NassauBack,
        Nassau18 = b.Nassau18,
        TotalStrokesBetPerStroke = b.TotalStrokesBetPerStroke,
        MaxNetScore = b.MaxNetScore,
        InvestmentOffEnabled = b.InvestmentOffEnabled,
        InvestmentOffAmount = b.InvestmentOffAmount,
        RedemptionEnabled = b.RedemptionEnabled,
        RedemptionAmount = b.RedemptionAmount,
        DunnEnabled = b.DunnEnabled,
        DunnAmount = b.DunnAmount,
        AutoPressEnabled = b.AutoPressEnabled,
        PressAmount = b.PressAmount,
        PressDownThreshold = b.PressDownThreshold,
        SkinsBuyIn = b.SkinsBuyIn,
        SkinsPerSkinAmount = b.SkinsPerSkinAmount,
        ExpenseDeductionPct = b.ExpenseDeductionPct,
        ScoresCountingPerHole = b.ScoresCountingPerHole,
        ConfigJson = b.ConfigJson,
        Teams = b.Teams.OrderBy(t => t.TeamNumber).Select(t => new TeamDto
        {
            TeamId = t.TeamId,
            TeamNumber = t.TeamNumber,
            TeamName = t.TeamName,
            Players = t.Players.Select(p => new TeamPlayerDto
            {
                TeamPlayerId = p.TeamPlayerId,
                PlayerId = p.PlayerId,
                PlayerName = p.Player?.FullName ?? string.Empty,
                Position = p.Position,
            }).ToList()
        }).ToList()
    };
}
