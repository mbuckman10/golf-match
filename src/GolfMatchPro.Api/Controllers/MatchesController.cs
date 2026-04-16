using GolfMatchPro.Data;
using GolfMatchPro.Data.Entities;
using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Shared.Dtos;
using GolfMatchPro.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController(GolfMatchDbContext db, IHandicapCalculator handicapCalc) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MatchDto>>> GetAll(
        [FromQuery] MatchStatus? status = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] bool includeArchived = false)
    {
        var query = db.Matches
            .Include(m => m.Course)
            .Include(m => m.Scores)
            .AsQueryable();

        if (!includeArchived)
            query = query.Where(m => !m.IsArchived);
        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);
        if (from.HasValue)
            query = query.Where(m => m.MatchDate >= from.Value);
        if (to.HasValue)
            query = query.Where(m => m.MatchDate <= to.Value);

        var matches = await query
            .OrderByDescending(m => m.MatchDate)
            .ThenByDescending(m => m.MatchId)
            .ToListAsync();

        return matches.Select(MapToDto).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MatchDetailDto>> GetById(int id)
    {
        var match = await db.Matches
            .Include(m => m.Course)
                .ThenInclude(c => c.Holes)
            .Include(m => m.Scores)
                .ThenInclude(s => s.Player)
            .FirstOrDefaultAsync(m => m.MatchId == id);

        if (match is null)
            return NotFound();

        return MapToDetailDto(match);
    }

    [HttpPost]
    public async Task<ActionResult<MatchDetailDto>> Create(CreateMatchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MatchName))
            return BadRequest(new { error = "Match name is required." });

        var course = await db.Courses
            .Include(c => c.Holes)
            .FirstOrDefaultAsync(c => c.CourseId == request.CourseId);

        if (course is null)
            return BadRequest(new { error = "Course not found." });

        var creator = await db.Players.FindAsync(request.CreatedByPlayerId);
        if (creator is null)
            return BadRequest(new { error = "Creator player not found." });

        var match = new Match
        {
            MatchName = request.MatchName.Trim(),
            CourseId = request.CourseId,
            MatchDate = request.MatchDate,
            CreatedByPlayerId = request.CreatedByPlayerId,
            Status = MatchStatus.Setup
        };

        db.Matches.Add(match);
        await db.SaveChangesAsync();

        // Enroll players
        if (request.PlayerIds.Count > 0)
        {
            var players = await db.Players
                .Where(p => request.PlayerIds.Contains(p.PlayerId))
                .ToListAsync();

            foreach (var player in players)
            {
                var courseHandicap = handicapCalc.ComputeCourseHandicap(player.HandicapIndex, course.SlopeRating);
                db.MatchScores.Add(new MatchScore
                {
                    MatchId = match.MatchId,
                    PlayerId = player.PlayerId,
                    CourseHandicap = courseHandicap
                });
            }
            await db.SaveChangesAsync();
        }

        // Reload with all includes
        var created = await db.Matches
            .Include(m => m.Course)
                .ThenInclude(c => c.Holes)
            .Include(m => m.Scores)
                .ThenInclude(s => s.Player)
            .FirstAsync(m => m.MatchId == match.MatchId);

        return CreatedAtAction(nameof(GetById), new { id = match.MatchId }, MapToDetailDto(created));
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateMatchStatusRequest request)
    {
        var match = await db.Matches.FindAsync(id);
        if (match is null)
            return NotFound();

        // Validate transitions: Setup → InProgress → Completed
        bool validTransition = (match.Status, request.Status) switch
        {
            (MatchStatus.Setup, MatchStatus.InProgress) => true,
            (MatchStatus.InProgress, MatchStatus.Completed) => true,
            _ => false
        };

        if (!validTransition)
            return BadRequest(new { error = $"Cannot transition from {match.Status} to {request.Status}." });

        match.Status = request.Status;
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{matchId:int}/players")]
    public async Task<ActionResult<List<MatchScoreDto>>> AddPlayers(int matchId, [FromBody] AddMatchPlayersRequest request)
    {
        var match = await db.Matches
            .Include(m => m.Course)
            .Include(m => m.Scores)
            .FirstOrDefaultAsync(m => m.MatchId == matchId);

        if (match is null)
            return NotFound();
        if (match.Status != MatchStatus.Setup)
            return BadRequest(new { error = "Can only add players during Setup." });

        var existingPlayerIds = match.Scores.Select(s => s.PlayerId).ToHashSet();
        var newPlayerIds = request.PlayerIds.Where(id => !existingPlayerIds.Contains(id)).ToList();

        var players = await db.Players
            .Where(p => newPlayerIds.Contains(p.PlayerId))
            .ToListAsync();

        var added = new List<MatchScore>();
        foreach (var player in players)
        {
            var courseHandicap = handicapCalc.ComputeCourseHandicap(player.HandicapIndex, match.Course.SlopeRating);
            var score = new MatchScore
            {
                MatchId = matchId,
                PlayerId = player.PlayerId,
                CourseHandicap = courseHandicap
            };
            db.MatchScores.Add(score);
            added.Add(score);
        }

        await db.SaveChangesAsync();

        // Reload to get player nav properties
        var scoreIds = added.Select(s => s.MatchScoreId).ToList();
        var loaded = await db.MatchScores
            .Include(s => s.Player)
            .Where(s => scoreIds.Contains(s.MatchScoreId))
            .ToListAsync();

        return loaded.Select(MapToScoreDto).ToList();
    }

    [HttpDelete("{matchId:int}/players/{playerId:int}")]
    public async Task<IActionResult> RemovePlayer(int matchId, int playerId)
    {
        var match = await db.Matches.FindAsync(matchId);
        if (match is null)
            return NotFound();
        if (match.Status != MatchStatus.Setup)
            return BadRequest(new { error = "Can only remove players during Setup." });

        var score = await db.MatchScores
            .FirstOrDefaultAsync(s => s.MatchId == matchId && s.PlayerId == playerId);
        if (score is null)
            return NotFound();

        db.MatchScores.Remove(score);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var match = await db.Matches
            .Include(m => m.Scores)
            .Include(m => m.Bets)
            .FirstOrDefaultAsync(m => m.MatchId == id);

        if (match is null)
            return NotFound();

        db.Matches.Remove(match);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id:int}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var match = await db.Matches.FindAsync(id);
        if (match is null)
            return NotFound();
        if (match.Status != MatchStatus.Completed)
            return BadRequest(new { error = "Only completed matches can be archived." });

        match.IsArchived = true;
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static MatchDto MapToDto(Match m) => new()
    {
        MatchId = m.MatchId,
        MatchName = m.MatchName,
        CourseId = m.CourseId,
        CourseName = m.Course.Name,
        CourseTeeColor = m.Course.TeeColor,
        MatchDate = m.MatchDate,
        Status = m.Status,
        IsArchived = m.IsArchived,
        CreatedByPlayerId = m.CreatedByPlayerId,
        PlayerCount = m.Scores.Count
    };

    private static MatchDetailDto MapToDetailDto(Match m) => new()
    {
        MatchId = m.MatchId,
        MatchName = m.MatchName,
        Course = new CourseDto
        {
            CourseId = m.Course.CourseId,
            Name = m.Course.Name,
            TeeColor = m.Course.TeeColor,
            YearOfInfo = m.Course.YearOfInfo,
            CourseRating = m.Course.CourseRating,
            SlopeRating = m.Course.SlopeRating,
            Par = m.Course.Holes.Sum(h => h.Par),
            Holes = m.Course.Holes.OrderBy(h => h.HoleNumber).Select(h => new CourseHoleDto
            {
                CourseHoleId = h.CourseHoleId,
                HoleNumber = h.HoleNumber,
                Par = h.Par,
                HandicapRanking = h.HandicapRanking
            }).ToList()
        },
        MatchDate = m.MatchDate,
        Status = m.Status,
        IsArchived = m.IsArchived,
        CreatedByPlayerId = m.CreatedByPlayerId,
        Scores = m.Scores.Select(MapToScoreDto).ToList()
    };

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

public class UpdateMatchStatusRequest
{
    public MatchStatus Status { get; set; }
}

public class AddMatchPlayersRequest
{
    public List<int> PlayerIds { get; set; } = [];
}
