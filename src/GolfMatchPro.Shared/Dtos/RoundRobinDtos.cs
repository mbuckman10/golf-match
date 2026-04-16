namespace GolfMatchPro.Shared.Dtos;

/// <summary>
/// Represents a single matchup result in a round robin.
/// </summary>
public class MatchupResultDto
{
    public int Matchup { get; set; }
    public int EntityAId { get; set; }
    public string EntityAName { get; set; } = string.Empty;
    public int EntityBId { get; set; }
    public string EntityBName { get; set; } = string.Empty;
    public decimal EntityAWinLoss { get; set; }
    public decimal EntityBWinLoss { get; set; }
    public string? ResultDetails { get; set; }
}

/// <summary>
/// Represents a leaderboard entry in a round robin.
/// </summary>
public class LeaderboardEntryDto
{
    public int EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public decimal TotalWinLoss { get; set; }
    public int Rank { get; set; }
    public int MatchupsPlayed { get; set; }
}

/// <summary>
/// Complete round robin result with matchups and leaderboard.
/// </summary>
public class RoundRobinResultDto
{
    public int RoundRobinId { get; set; }
    public int MatchId { get; set; }
    public int BetConfigId { get; set; }
    public List<MatchupResultDto> Matchups { get; set; } = [];
    public List<LeaderboardEntryDto> Leaderboard { get; set; } = [];
}

/// <summary>
/// Request to calculate round robin results.
/// </summary>
public class CalculateRoundRobinRequest
{
    public int BetConfigId { get; set; }
}
