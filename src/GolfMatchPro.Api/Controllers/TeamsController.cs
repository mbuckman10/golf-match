using GolfMatchPro.Data;
using GolfMatchPro.Data.Entities;
using GolfMatchPro.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId:int}/bets/{betConfigId:int}/teams")]
public class TeamsController(GolfMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TeamDto>>> GetAll(int matchId, int betConfigId)
    {
        var config = await db.BetConfigurations
            .FirstOrDefaultAsync(b => b.BetConfigId == betConfigId && b.MatchId == matchId);
        if (config is null) return NotFound();

        var teams = await db.Teams
            .Include(t => t.Players)
                .ThenInclude(tp => tp.Player)
            .Where(t => t.BetConfigId == betConfigId)
            .OrderBy(t => t.TeamNumber)
            .ToListAsync();

        return teams.Select(MapToDto).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<TeamDto>> Create(int matchId, int betConfigId, CreateTeamRequest request)
    {
        var config = await db.BetConfigurations
            .FirstOrDefaultAsync(b => b.BetConfigId == betConfigId && b.MatchId == matchId);
        if (config is null) return NotFound();

        // Verify all players are enrolled in the match
        var enrolledPlayerIds = await db.MatchScores
            .Where(ms => ms.MatchId == matchId)
            .Select(ms => ms.PlayerId)
            .ToListAsync();

        foreach (var playerReq in request.Players)
        {
            if (!enrolledPlayerIds.Contains(playerReq.PlayerId))
                return BadRequest($"Player {playerReq.PlayerId} is not enrolled in this match.");
        }

        var team = new Team
        {
            BetConfigId = betConfigId,
            TeamNumber = request.TeamNumber,
            TeamName = request.TeamName,
            Players = request.Players.Select(p => new TeamPlayer
            {
                PlayerId = p.PlayerId,
                Position = p.Position,
            }).ToList()
        };

        db.Teams.Add(team);
        await db.SaveChangesAsync();

        // Reload with player names
        await db.Entry(team).Collection(t => t.Players).LoadAsync();
        foreach (var tp in team.Players)
            await db.Entry(tp).Reference(p => p.Player).LoadAsync();

        return CreatedAtAction(nameof(GetAll), new { matchId, betConfigId }, MapToDto(team));
    }

    [HttpPut("{teamId:int}")]
    public async Task<ActionResult<TeamDto>> Update(int matchId, int betConfigId, int teamId, CreateTeamRequest request)
    {
        var team = await db.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.TeamId == teamId && t.BetConfigId == betConfigId);

        if (team is null) return NotFound();

        // Verify the bet config belongs to this match
        var config = await db.BetConfigurations
            .FirstOrDefaultAsync(b => b.BetConfigId == betConfigId && b.MatchId == matchId);
        if (config is null) return NotFound();

        team.TeamNumber = request.TeamNumber;
        team.TeamName = request.TeamName;

        // Replace players
        db.TeamPlayers.RemoveRange(team.Players);
        team.Players = request.Players.Select(p => new TeamPlayer
        {
            TeamId = teamId,
            PlayerId = p.PlayerId,
            Position = p.Position,
        }).ToList();

        await db.SaveChangesAsync();

        // Reload with player names
        foreach (var tp in team.Players)
            await db.Entry(tp).Reference(p => p.Player).LoadAsync();

        return MapToDto(team);
    }

    [HttpDelete("{teamId:int}")]
    public async Task<IActionResult> Delete(int matchId, int betConfigId, int teamId)
    {
        var team = await db.Teams
            .FirstOrDefaultAsync(t => t.TeamId == teamId && t.BetConfigId == betConfigId);

        if (team is null) return NotFound();

        var config = await db.BetConfigurations
            .FirstOrDefaultAsync(b => b.BetConfigId == betConfigId && b.MatchId == matchId);
        if (config is null) return NotFound();

        db.Teams.Remove(team);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private static TeamDto MapToDto(Team t) => new()
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
    };
}
