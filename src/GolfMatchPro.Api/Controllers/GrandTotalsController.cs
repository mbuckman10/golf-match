using GolfMatchPro.Engine.GrandTotals;
using GolfMatchPro.Data;
using GolfMatchPro.Data.Entities;
using GolfMatchPro.Shared.Enums;
using GolfMatchPro.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId}/grand-totals")]
public class GrandTotalsController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IGrandTotalCalculator _grandTotalCalculator;
    private readonly GolfMatchDbContext _dbContext;
    private readonly ILogger<GrandTotalsController> _logger;

    public GrandTotalsController(
        IGrandTotalCalculator grandTotalCalculator,
        GolfMatchDbContext dbContext,
        ILogger<GrandTotalsController> logger)
    {
        _grandTotalCalculator = grandTotalCalculator;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Calculate grand totals for all players in a match.
    /// Aggregates across all selected bet types with optional filters.
    /// </summary>
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateGrandTotals(
        int matchId,
        [FromBody] CalculateGrandTotalsRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Calculating Grand Totals for match {MatchId} with filters: " +
                "Foursomes={Foursomes}, Threesomes={Threesomes}, Individual={Individual}, " +
                "BestBall={BestBall}, SkinsGross={SkinsGross}, Tournament={Tournament}",
                matchId,
                request.IncludeFoursomes,
                request.IncludeThreesomes,
                request.IncludeIndividual,
                request.IncludeBestBall,
                request.IncludeSkinsGross,
                request.IncludeIndoTourney);

            var playersInMatch = await _dbContext.MatchScores
                .AsNoTracking()
                .Include(ms => ms.Player)
                .Where(ms => ms.MatchId == matchId)
                .Select(ms => ms.Player)
                .Distinct()
                .ToListAsync();

            var betResults = await _dbContext.BetResults
                .AsNoTracking()
                .Include(br => br.BetConfiguration)
                .Where(br => br.BetConfiguration.MatchId == matchId)
                .ToListAsync();

            var roundRobinResults = await _dbContext.RoundRobinResults
                .AsNoTracking()
                .Where(rr => rr.MatchId == matchId)
                .ToListAsync();

            var playerTotals = new List<PlayerGrandTotalDto>();

            foreach (var player in playersInMatch)
            {
                var betTypeAmounts = BuildBetTypeAmountsForPlayer(player.PlayerId, betResults);
                betTypeAmounts["RoundRobin"] = CalculateRoundRobinAmountForPlayer(player.PlayerId, roundRobinResults);

                var total = _grandTotalCalculator.Calculate(
                    player.PlayerId,
                    player.Nickname ?? player.FullName,
                    betTypeAmounts,
                    request);

                playerTotals.Add(total);
            }

            var existingTotals = await _dbContext.GrandTotals
                .Where(gt => gt.MatchId == matchId)
                .ToListAsync();

            foreach (var total in playerTotals)
            {
                var row = existingTotals.FirstOrDefault(gt => gt.PlayerId == total.PlayerId);
                if (row is null)
                {
                    row = new GrandTotal
                    {
                        MatchId = matchId,
                        PlayerId = total.PlayerId
                    };
                    _dbContext.GrandTotals.Add(row);
                }

                row.IncludeFoursomes = request.IncludeFoursomes;
                row.IncludeThreesomes = request.IncludeThreesomes;
                row.IncludeFivesomes = request.IncludeFivesomes;
                row.IncludeIndividual = request.IncludeIndividual;
                row.IncludeBestBall = request.IncludeBestBall;
                row.IncludeSkinsGross = request.IncludeSkinsGross;
                row.IncludeSkinsNet = request.IncludeSkinsNet;
                row.IncludeIndoTourney = request.IncludeIndoTourney;
                row.IncludeRoundRobins = request.IncludeRoundRobins;
                row.TotalWinLoss = total.TotalWinLoss;
                row.DetailJson = JsonSerializer.Serialize(total, JsonOptions);
                row.LastUpdatedUtc = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();

            var result = new GrandTotalsDto
            {
                MatchId = matchId,
                PlayerTotals = playerTotals.OrderByDescending(p => p.TotalWinLoss).ToList()
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Grand Totals for match {MatchId}", matchId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get grand totals grouped by bet type for analysis.
    /// </summary>
    [HttpGet("by-bet-type")]
    public async Task<IActionResult> GetGrandTotalsByBetType(
        int matchId,
        [FromQuery] bool includeFoursomes = true,
        [FromQuery] bool includeThreesomes = true,
        [FromQuery] bool includeFivesomes = true,
        [FromQuery] bool includeIndividual = true,
        [FromQuery] bool includeBestBall = true,
        [FromQuery] bool includeSkinsGross = true,
        [FromQuery] bool includeSkinsNet = true,
        [FromQuery] bool includeIndoTourney = true,
        [FromQuery] bool includeRoundRobins = true)
    {
        try
        {
            var request = new CalculateGrandTotalsRequest
            {
                IncludeFoursomes = includeFoursomes,
                IncludeThreesomes = includeThreesomes,
                IncludeFivesomes = includeFivesomes,
                IncludeIndividual = includeIndividual,
                IncludeBestBall = includeBestBall,
                IncludeSkinsGross = includeSkinsGross,
                IncludeSkinsNet = includeSkinsNet,
                IncludeIndoTourney = includeIndoTourney,
                IncludeRoundRobins = includeRoundRobins
            };

            _logger.LogInformation("Fetching Grand Totals by Bet Type for match {MatchId}", matchId);

            return await CalculateGrandTotals(matchId, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Grand Totals by Bet Type for match {MatchId}", matchId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get grand totals for a specific player in a match.
    /// </summary>
    [HttpGet("player/{playerId}")]
    public async Task<IActionResult> GetPlayerGrandTotal(int matchId, int playerId)
    {
        try
        {
            _logger.LogInformation(
                "Fetching Grand Total for player {PlayerId} in match {MatchId}",
                playerId,
                matchId);

            var total = await _dbContext.GrandTotals
                .AsNoTracking()
                .Where(gt => gt.MatchId == matchId && gt.PlayerId == playerId)
                .Select(gt => gt.DetailJson)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(total))
            {
                return NotFound(new { error = "Grand total not found for player. Run calculate first." });
            }

            var dto = JsonSerializer.Deserialize<PlayerGrandTotalDto>(total, JsonOptions);
            if (dto is null)
            {
                return NotFound(new { error = "Stored grand total payload is invalid." });
            }

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Grand Total for player {PlayerId}", playerId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get leaderboard ranking by grand total winnings/losses.
    /// </summary>
    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard(int matchId)
    {
        try
        {
            _logger.LogInformation("Fetching Grand Totals Leaderboard for match {MatchId}", matchId);

            var serialized = await _dbContext.GrandTotals
                .AsNoTracking()
                .Where(gt => gt.MatchId == matchId)
                .Select(gt => gt.DetailJson)
                .ToListAsync();

            var leaderboard = serialized
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => JsonSerializer.Deserialize<PlayerGrandTotalDto>(s!, JsonOptions))
                .Where(s => s is not null)
                .Select(s => s!)
                .OrderByDescending(s => s.TotalWinLoss)
                .ToList();

            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Grand Totals Leaderboard for match {MatchId}", matchId);
            return BadRequest(new { error = ex.Message });
        }
    }

    private static Dictionary<string, decimal> BuildBetTypeAmountsForPlayer(int playerId, List<BetResult> betResults)
    {
        var amounts = new Dictionary<string, decimal>
        {
            ["Foursomes"] = 0,
            ["Threesomes"] = 0,
            ["Fivesomes"] = 0,
            ["Individual"] = 0,
            ["BestBall"] = 0,
            ["SkinsGross"] = 0,
            ["SkinsNet"] = 0,
            ["Tournament"] = 0,
            ["RoundRobin"] = 0
        };

        foreach (var result in betResults.Where(r => r.PlayerId == playerId))
        {
            switch (result.BetConfiguration.BetType)
            {
                case BetType.Foursome:
                    amounts["Foursomes"] += result.WinLossAmount;
                    break;
                case BetType.Threesome:
                    amounts["Threesomes"] += result.WinLossAmount;
                    break;
                case BetType.Fivesome:
                    amounts["Fivesomes"] += result.WinLossAmount;
                    break;
                case BetType.Individual:
                    amounts["Individual"] += result.WinLossAmount + (result.PressResult ?? 0m);
                    break;
                case BetType.BestBall:
                    amounts["BestBall"] += result.WinLossAmount;
                    break;
                case BetType.Skins:
                    amounts["SkinsGross"] += result.SkinsAmount ?? 0m;
                    break;
                case BetType.IndoTournament:
                    amounts["Tournament"] += result.WinLossAmount;
                    break;
                case BetType.RoundRobin:
                    amounts["RoundRobin"] += result.WinLossAmount;
                    break;
            }
        }

        return amounts;
    }

    private static decimal CalculateRoundRobinAmountForPlayer(int playerId, List<RoundRobinResult> roundRobinResults)
    {
        decimal total = 0m;

        foreach (var rr in roundRobinResults)
        {
            var matchups = JsonSerializer.Deserialize<List<MatchupResultDto>>(rr.MatchupsJson, JsonOptions) ?? [];
            total += matchups.Where(m => m.EntityAId == playerId).Sum(m => m.EntityAWinLoss);
            total += matchups.Where(m => m.EntityBId == playerId).Sum(m => m.EntityBWinLoss);
        }

        return total;
    }
}
