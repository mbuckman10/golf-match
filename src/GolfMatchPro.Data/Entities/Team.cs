using System.ComponentModel.DataAnnotations;

namespace GolfMatchPro.Data.Entities;

public class Team
{
    public int TeamId { get; set; }

    public int BetConfigId { get; set; }
    public BetConfiguration BetConfiguration { get; set; } = null!;

    public int TeamNumber { get; set; }

    [MaxLength(100)]
    public string? TeamName { get; set; }

    public ICollection<TeamPlayer> Players { get; set; } = [];
}
