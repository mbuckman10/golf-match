using System.ComponentModel.DataAnnotations;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Data.Entities;

public class Match
{
    public int MatchId { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateOnly MatchDate { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Setup;

    public int CreatedByPlayerId { get; set; }
    public Player CreatedBy { get; set; } = null!;

    public ICollection<MatchScore> Scores { get; set; } = [];
    public ICollection<BetConfiguration> Bets { get; set; } = [];
}
