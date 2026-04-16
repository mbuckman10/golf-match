using GolfMatchPro.Engine.RoundRobin;
using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Data;
using GolfMatchPro.Data.Entities;
using GolfMatchPro.Shared.Enums;
using GolfMatchPro.Shared.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId}/round-robin")]
public class RoundRobinController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IRoundRobinCalculator _roundRobinCalculator;
    private readonly IHandicapCalculator _handicapCalculator;
    private readonly GolfMatchDbContext _dbContext;
    private readonly ILogger<RoundRobinController> _logger;

    public RoundRobinController(
        IRoundRobinCalculator roundRobinCalculator,
        IHandicapCalculator handicapCalculator,
        GolfMatchDbContext dbContext,
        ILogger<RoundRobinController> logger)
    {
        _roundRobinCalculator = roundRobinCalculator;
        _handicapCalculator = handicapCalculator;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Calculate foursome round robin results for a match.
    /// Every team plays every other team.
    /// </summary>
    [HttpPost("foursomes/calculate")]
    public async Task<IActionResult> CalculateFoursomeRoundRobin(
        int matchId,
        [FromBody] RoundRobinCalculationRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating Foursome Round Robin for match {MatchId}", matchId);

            var betConfig = await _dbContext.BetConfigurations
                .Include(b => b.Teams)
                    .ThenInclude(t => t.Players)
                .Include(b => b.Match)
                    .ThenInclude(m => m.Course)
                        .ThenInclude(c => c.Holes)
                .FirstOrDefaultAsync(b => b.BetConfigId == request.BetConfigId && b.MatchId == matchId);

            if (betConfig is null)
            {
                return NotFound(new { error = "Bet configuration not found for this match." });
            }

            var scoreMap = await _dbContext.MatchScores
                .Include(s => s.Player)
                .Where(s => s.MatchId == matchId)
                .ToDictionaryAsync(s => s.PlayerId, s => s);

            var teamData = BuildTeamRoundRobinData(betConfig, scoreMap, betConfig.ScoresCountingPerHole > 0 ? betConfig.ScoresCountingPerHole : 2);

            var result = _roundRobinCalculator.CalculateFoursomeRoundRobin(
                teamData,
                betConfig.NassauFront,
                betConfig.NassauBack,
                betConfig.Nassau18,
                betConfig.InvestmentOffEnabled,
                betConfig.InvestmentOffAmount,
                betConfig.RedemptionEnabled,
                betConfig.RedemptionAmount);

            var entity = new RoundRobinResult
            {
                MatchId = matchId,
                BetConfigId = request.BetConfigId,
                RoundRobinType = "Foursome",
                MatchupsJson = JsonSerializer.Serialize(result.Matchups, JsonOptions),
                LeaderboardJson = JsonSerializer.Serialize(result.Leaderboard, JsonOptions),
                CalculatedAtUtc = DateTime.UtcNow
            };

            _dbContext.RoundRobinResults.Add(entity);
            await _dbContext.SaveChangesAsync();

            result.MatchId = matchId;
            result.BetConfigId = request.BetConfigId;
            result.RoundRobinId = entity.RoundRobinResultId;

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Foursome Round Robin for match {MatchId}", matchId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Calculate individual round robin results for a match.
    /// Every player plays every other player.
    /// </summary>
    [HttpPost("individual/calculate")]
    public async Task<IActionResult> CalculateIndividualRoundRobin(
        int matchId,
        [FromBody] RoundRobinCalculationRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating Individual Round Robin for match {MatchId}", matchId);

            var betConfig = await _dbContext.BetConfigurations
                .FirstOrDefaultAsync(b => b.BetConfigId == request.BetConfigId && b.MatchId == matchId);

            if (betConfig is null)
            {
                return NotFound(new { error = "Bet configuration not found for this match." });
            }

            var players = await _dbContext.MatchScores
                .Include(s => s.Player)
                .Where(s => s.MatchId == matchId)
                .Select(s => new PlayerRoundRobinData
                {
                    PlayerId = s.PlayerId,
                    PlayerName = s.Player.Nickname ?? s.Player.FullName,
                    NetScores = s.GetHoleScores()
                })
                .ToListAsync();

            var result = _roundRobinCalculator.CalculateIndividualRoundRobin(
                players,
                betConfig.NassauFront,
                betConfig.NassauBack,
                betConfig.Nassau18,
                betConfig.AutoPressEnabled,
                betConfig.PressAmount,
                betConfig.PressDownThreshold);

            var entity = new RoundRobinResult
            {
                MatchId = matchId,
                BetConfigId = request.BetConfigId,
                RoundRobinType = "Individual",
                MatchupsJson = JsonSerializer.Serialize(result.Matchups, JsonOptions),
                LeaderboardJson = JsonSerializer.Serialize(result.Leaderboard, JsonOptions),
                CalculatedAtUtc = DateTime.UtcNow
            };

            _dbContext.RoundRobinResults.Add(entity);
            await _dbContext.SaveChangesAsync();

            result.MatchId = matchId;
            result.BetConfigId = request.BetConfigId;
            result.RoundRobinId = entity.RoundRobinResultId;

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Individual Round Robin for match {MatchId}", matchId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Calculate best ball round robin results for a match.
    /// Every 2-man team plays every other 2-man team.
    /// </summary>
    [HttpPost("bestball/calculate")]
    public async Task<IActionResult> CalculateBestBallRoundRobin(
        int matchId,
        [FromBody] RoundRobinCalculationRequest request)
    {
        try
        {
            _logger.LogInformation("Calculating Best Ball Round Robin for match {MatchId}", matchId);

            var betConfig = await _dbContext.BetConfigurations
                .Include(b => b.Teams)
                    .ThenInclude(t => t.Players)
                .Include(b => b.Match)
                    .ThenInclude(m => m.Course)
                        .ThenInclude(c => c.Holes)
                .FirstOrDefaultAsync(b => b.BetConfigId == request.BetConfigId && b.MatchId == matchId);

            if (betConfig is null)
            {
                return NotFound(new { error = "Bet configuration not found for this match." });
            }

            var scoreMap = await _dbContext.MatchScores
                .Include(s => s.Player)
                .Where(s => s.MatchId == matchId)
                .ToDictionaryAsync(s => s.PlayerId, s => s);

            var teamData = BuildTeamRoundRobinData(betConfig, scoreMap, 1);

            var result = _roundRobinCalculator.CalculateBestBallRoundRobin(
                teamData,
                betConfig.NassauFront,
                betConfig.NassauBack,
                betConfig.Nassau18);

            var entity = new RoundRobinResult
            {
                MatchId = matchId,
                BetConfigId = request.BetConfigId,
                RoundRobinType = "BestBall",
                MatchupsJson = JsonSerializer.Serialize(result.Matchups, JsonOptions),
                LeaderboardJson = JsonSerializer.Serialize(result.Leaderboard, JsonOptions),
                CalculatedAtUtc = DateTime.UtcNow
            };

            _dbContext.RoundRobinResults.Add(entity);
            await _dbContext.SaveChangesAsync();

            result.MatchId = matchId;
            result.BetConfigId = request.BetConfigId;
            result.RoundRobinId = entity.RoundRobinResultId;

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating Best Ball Round Robin for match {MatchId}", matchId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get previously calculated round robin results.
    /// </summary>
    [HttpGet("{roundRobinId}")]
    public async Task<IActionResult> GetRoundRobinResults(int matchId, int roundRobinId)
    {
        try
        {
            _logger.LogInformation("Fetching Round Robin {RoundRobinId} for match {MatchId}", roundRobinId, matchId);

            var entity = await _dbContext.RoundRobinResults
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoundRobinResultId == roundRobinId && r.MatchId == matchId);

            if (entity is null)
            {
                return NotFound(new { error = "Round robin result not found." });
            }

            var dto = new RoundRobinResultDto
            {
                RoundRobinId = entity.RoundRobinResultId,
                MatchId = entity.MatchId,
                BetConfigId = entity.BetConfigId,
                Matchups = JsonSerializer.Deserialize<List<MatchupResultDto>>(entity.MatchupsJson, JsonOptions) ?? [],
                Leaderboard = JsonSerializer.Deserialize<List<LeaderboardEntryDto>>(entity.LeaderboardJson, JsonOptions) ?? []
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Round Robin {RoundRobinId}", roundRobinId);
            return BadRequest(new { error = ex.Message });
        }
    }
    private List<TeamRoundRobinData> BuildTeamRoundRobinData(
        BetConfiguration betConfig,
        Dictionary<int, MatchScore> scoreMap,
        int scoresCountingPerHole)
    {
        var holePars = betConfig.Match.Course.Holes
            .OrderBy(h => h.HoleNumber)
            .Select(h => h.Par)
            .ToArray();

        var holeRankings = betConfig.Match.Course.Holes
            .OrderBy(h => h.HoleNumber)
            .Select(h => h.HandicapRanking)
            .ToArray();

        if (holePars.Length != 18 || holeRankings.Length != 18)
        {
            // Safe fallback for partial course setup during early testing.
            holePars = Enumerable.Repeat(4, 18).ToArray();
            holeRankings = Enumerable.Range(1, 18).ToArray();
        }

        var teamPlayers = betConfig.Teams
            .Select(t => t.Players
                .Where(tp => scoreMap.ContainsKey(tp.PlayerId))
                .Select(tp => scoreMap[tp.PlayerId]))
            .SelectMany(s => s)
            .ToList();

        if (teamPlayers.Count == 0)
        {
            return [];
        }

        var lowestCourseHandicap = teamPlayers.Min(s => s.CourseHandicap);
        var handicapPct = betConfig.HandicapPercentage / 100m;

        var teams = betConfig.Teams
            .Select(team =>
            {
                var playerScores = team.Players
                    .Where(tp => scoreMap.ContainsKey(tp.PlayerId))
                    .Select(tp => scoreMap[tp.PlayerId])
                    .ToList();

                var grossScores = playerScores
                    .Select(ps => ps.GetHoleScores())
                    .ToList();

                var handicapStrokes = playerScores
                    .Select(ps =>
                    {
                        var playingHandicap = _handicapCalculator.ComputePlayingHandicap(
                            ps.CourseHandicap,
                            handicapPct,
                            lowestCourseHandicap);

                        return _handicapCalculator.DistributeStrokes(playingHandicap, holeRankings);
                    })
                    .ToList();

                var netScores = new List<int[]>();
                for (int i = 0; i < grossScores.Count; i++)
                {
                    var net = new int[18];
                    for (int h = 0; h < 18; h++)
                    {
                        if (grossScores[i][h] > 0)
                        {
                            net[h] = grossScores[i][h] - handicapStrokes[i][h];
                        }
                    }
                    netScores.Add(net);
                }

                return new TeamRoundRobinData
                {
                    TeamId = team.TeamId,
                    TeamName = team.TeamName ?? $"Team {team.TeamNumber}",
                    ScoresCountingPerHole = scoresCountingPerHole,
                    PlayerNetScores = netScores,
                    PlayerGrossScores = grossScores,
                    PlayerHandicapStrokes = handicapStrokes,
                    HolePars = holePars
                };
            })
            .Where(t => t.PlayerNetScores.Count > 0)
            .ToList();

        return teams;
    }
}

/// <summary>
/// Request body for round robin calculations.
/// </summary>
public class RoundRobinCalculationRequest
{
    public int BetConfigId { get; set; }
}
