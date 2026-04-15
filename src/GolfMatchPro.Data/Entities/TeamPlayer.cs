using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Data.Entities;

public class TeamPlayer
{
    public int TeamPlayerId { get; set; }

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public TeamPosition Position { get; set; }
}
