namespace GolfMatchPro.Data.Entities;

public class GrandTotal
{
    public int GrandTotalId { get; set; }

    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public bool IncludeFoursomes { get; set; } = true;
    public bool IncludeThreesomes { get; set; } = true;
    public bool IncludeFivesomes { get; set; } = true;
    public bool IncludeIndividual { get; set; } = true;
    public bool IncludeBestBall { get; set; } = true;
    public bool IncludeSkinsGross { get; set; } = true;
    public bool IncludeSkinsNet { get; set; } = true;
    public bool IncludeIndoTourney { get; set; } = true;
    public bool IncludeRoundRobins { get; set; } = true;

    public decimal TotalWinLoss { get; set; }
    public string? DetailJson { get; set; }

    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}
