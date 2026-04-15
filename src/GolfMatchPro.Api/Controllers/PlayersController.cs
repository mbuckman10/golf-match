using GolfMatchPro.Data;
using GolfMatchPro.Data.Entities;
using GolfMatchPro.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController(GolfMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PlayerDto>>> GetAll([FromQuery] string? search)
    {
        var query = db.Players.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.FullName.Contains(term) ||
                (p.Nickname != null && p.Nickname.Contains(term)));
        }

        var players = await query
            .OrderBy(p => p.FullName)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return Ok(players);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDto>> GetById(int id)
    {
        var player = await db.Players.FindAsync(id);
        if (player is null) return NotFound();
        return Ok(MapToDto(player));
    }

    [HttpPost]
    public async Task<ActionResult<PlayerDto>> Create(CreatePlayerRequest request)
    {
        var errors = ValidateCreateRequest(request);
        if (errors.Count > 0) return BadRequest(new { errors });

        var player = new Player
        {
            FullName = request.FullName,
            Nickname = request.Nickname,
            HandicapIndex = request.HandicapIndex,
            IsActive = true,
            IsGuest = request.IsGuest
        };

        db.Players.Add(player);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = player.PlayerId }, MapToDto(player));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PlayerDto>> Update(int id, UpdatePlayerRequest request)
    {
        var player = await db.Players.FindAsync(id);
        if (player is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { errors = new[] { "Full name is required." } });

        if (request.HandicapIndex < -10 || request.HandicapIndex > 54)
            return BadRequest(new { errors = new[] { "Handicap index must be between -10 and 54." } });

        player.FullName = request.FullName;
        player.Nickname = request.Nickname;
        player.HandicapIndex = request.HandicapIndex;
        player.IsActive = request.IsActive;
        player.IsGuest = request.IsGuest;

        await db.SaveChangesAsync();
        return Ok(MapToDto(player));
    }

    private static List<string> ValidateCreateRequest(CreatePlayerRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FullName))
            errors.Add("Full name is required.");

        if (request.HandicapIndex < -10 || request.HandicapIndex > 54)
            errors.Add("Handicap index must be between -10 and 54.");

        return errors;
    }

    private static PlayerDto MapToDto(Player player) => new()
    {
        PlayerId = player.PlayerId,
        FullName = player.FullName,
        Nickname = player.Nickname,
        HandicapIndex = player.HandicapIndex,
        IsActive = player.IsActive,
        IsGuest = player.IsGuest
    };
}
