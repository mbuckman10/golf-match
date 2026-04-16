namespace GolfMatchPro.Shared.Dtos;

/// <summary>
/// Grand total winnings/losses for a single player across all bets.
/// </summary>
public class PlayerGrandTotalDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;

    // Per-bet-type breakdown
    public decimal FoursomesWinLoss { get; set; }
    public decimal ThreesomesWinLoss { get; set; }
    public decimal FivesomesWinLoss { get; set; }
    public decimal IndividualWinLoss { get; set; }
    public decimal BestBallWinLoss { get; set; }
    public decimal SkinsGrossWinLoss { get; set; }
    public decimal SkinsNetWinLoss { get; set; }
    public decimal IndoTourneyWinLoss { get; set; }
    public decimal RoundRobinWinLoss { get; set; }

    // Aggregate
    public decimal TotalWinLoss { get; set; }
    public string Status { get; set; } = string.Empty; // "Win", "Loss", "Break Even"
}

/// <summary>
/// Request to calculate grand totals with optional bet type filters.
/// </summary>
public class CalculateGrandTotalsRequest
{
    public bool IncludeFoursomes { get; set; } = true;
    public bool IncludeThreesomes { get; set; } = true;
    public bool IncludeFivesomes { get; set; } = true;
    public bool IncludeIndividual { get; set; } = true;
    public bool IncludeBestBall { get; set; } = true;
    public bool IncludeSkinsGross { get; set; } = true;
    public bool IncludeSkinsNet { get; set; } = true;
    public bool IncludeIndoTourney { get; set; } = true;
    public bool IncludeRoundRobins { get; set; } = true;
}

/// <summary>
/// Complete grand totals for all players in a match.
/// </summary>
public class GrandTotalsDto
{
    public int MatchId { get; set; }
    public List<PlayerGrandTotalDto> PlayerTotals { get; set; } = [];
}
