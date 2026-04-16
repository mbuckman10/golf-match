namespace GolfMatchPro.Data.Entities;

public class RoundRobinResult
{
    public int RoundRobinResultId { get; set; }

    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public int BetConfigId { get; set; }
    public BetConfiguration BetConfiguration { get; set; } = null!;

    public string RoundRobinType { get; set; } = string.Empty;

    public string MatchupsJson { get; set; } = "[]";
    public string LeaderboardJson { get; set; } = "[]";

    public DateTime CalculatedAtUtc { get; set; } = DateTime.UtcNow;
}
