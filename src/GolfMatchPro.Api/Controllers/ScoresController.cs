using GolfMatchPro.Data;
using GolfMatchPro.Data.Entities;
using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Shared.Dtos;
using GolfMatchPro.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/scores")]
public class ScoresController(
    GolfMatchDbContext db,
    IHandicapCalculator handicapCalc,
    IHubContext<Hubs.MatchHub> hubContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MatchScoreDto>>> GetAll(int matchId)
    {
        var match = await db.Matches.FindAsync(matchId);
        if (match is null) return NotFound();

        var scores = await db.MatchScores
            .Include(s => s.Player)
            .Where(s => s.MatchId == matchId)
            .ToListAsync();

        return scores.Select(MapToScoreDto).ToList();
    }

    [HttpGet("{playerId:int}")]
    public async Task<ActionResult<MatchScoreDto>> GetByPlayer(int matchId, int playerId)
    {
        var score = await db.MatchScores
            .Include(s => s.Player)
            .FirstOrDefaultAsync(s => s.MatchId == matchId && s.PlayerId == playerId);

        if (score is null) return NotFound();
        return MapToScoreDto(score);
    }

    [HttpPut("{playerId:int}")]
    public async Task<ActionResult<MatchScoreDto>> BulkUpdate(int matchId, int playerId, BulkUpdateScoreRequest request)
    {
        var score = await db.MatchScores
            .Include(s => s.Player)
            .Include(s => s.Match)
                .ThenInclude(m => m.Course)
                    .ThenInclude(c => c.Holes)
            .FirstOrDefaultAsync(s => s.MatchId == matchId && s.PlayerId == playerId);

        if (score is null) return NotFound();

        if (request.HoleScores.Length != 18)
            return BadRequest(new { error = "Must provide exactly 18 hole scores." });

        for (int i = 0; i < 18; i++)
        {
            if (request.HoleScores[i] < 0 || request.HoleScores[i] > 15)
                return BadRequest(new { error = $"Hole {i + 1} score must be 0-15." });
            if (request.HoleScores[i] > 0)
                score.SetHoleScore(i + 1, request.HoleScores[i]);
        }

        RecomputeTotals(score);
        await db.SaveChangesAsync();

        await hubContext.Clients.Group($"match-{matchId}")
            .SendAsync("ScoreUpdated", matchId, playerId, 0, 0);

        return MapToScoreDto(score);
    }

    [HttpPost("{playerId:int}/hole/{holeNumber:int}")]
    public async Task<ActionResult<HoleScoreResultDto>> UpdateHole(
        int matchId, int playerId, int holeNumber, [FromBody] UpdateScoreRequest request)
    {
        if (holeNumber < 1 || holeNumber > 18)
            return BadRequest(new { error = "Hole number must be 1-18." });
        if (request.Score < 1 || request.Score > 15)
            return BadRequest(new { error = "Score must be 1-15." });

        var score = await db.MatchScores
            .Include(s => s.Player)
            .Include(s => s.Match)
                .ThenInclude(m => m.Course)
                    .ThenInclude(c => c.Holes)
            .FirstOrDefaultAsync(s => s.MatchId == matchId && s.PlayerId == playerId);

        if (score is null) return NotFound();

        score.SetHoleScore(holeNumber, request.Score);
        RecomputeTotals(score);
        await db.SaveChangesAsync();

        await hubContext.Clients.Group($"match-{matchId}")
            .SendAsync("ScoreUpdated", matchId, playerId, holeNumber, request.Score);

        var holes = score.GetHoleScores();
        return new HoleScoreResultDto
        {
            PlayerId = playerId,
            HoleNumber = holeNumber,
            Score = request.Score,
            GrossTotal = score.GrossTotal,
            NetTotal = score.NetTotal,
            FrontNine = holes.Take(9).Where(s => s > 0).Sum(),
            BackNine = holes.Skip(9).Where(s => s > 0).Sum(),
            HolesCompleted = holes.Count(s => s > 0),
            IsComplete = score.IsComplete
        };
    }

    private void RecomputeTotals(MatchScore score)
    {
        var holeScores = score.GetHoleScores();
        var holePars = score.Match.Course.Holes
            .OrderBy(h => h.HoleNumber)
            .Select(h => h.Par)
            .ToArray();

        score.GrossTotal = holeScores.Where(s => s > 0).Sum();
        score.NetTotal = score.GrossTotal - score.CourseHandicap;
        score.ReportableScore = handicapCalc.ComputeReportableScore(holeScores, holePars, score.CourseHandicap);
        score.IsComplete = holeScores.All(s => s > 0);
    }

    private static MatchScoreDto MapToScoreDto(MatchScore s) => new()
    {
        MatchScoreId = s.MatchScoreId,
        PlayerId = s.PlayerId,
        PlayerName = s.Player.FullName,
        PlayerNickname = s.Player.Nickname,
        CourseHandicap = s.CourseHandicap,
        HoleScores = s.GetHoleScores(),
        GrossTotal = s.GrossTotal,
        NetTotal = s.NetTotal,
        ReportableScore = s.ReportableScore,
        IsComplete = s.IsComplete
    };
}

public class HoleScoreResultDto
{
    public int PlayerId { get; set; }
    public int HoleNumber { get; set; }
    public int Score { get; set; }
    public int GrossTotal { get; set; }
    public int NetTotal { get; set; }
    public int FrontNine { get; set; }
    public int BackNine { get; set; }
    public int HolesCompleted { get; set; }
    public bool IsComplete { get; set; }
}
