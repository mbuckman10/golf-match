using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Data.Entities;

public class BetConfiguration
{
    public int BetConfigId { get; set; }

    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public BetType BetType { get; set; }
    public CompetitionType CompetitionType { get; set; }

    public decimal HandicapPercentage { get; set; } = 100;

    public decimal NassauFront { get; set; } = 5;
    public decimal NassauBack { get; set; } = 5;
    public decimal Nassau18 { get; set; } = 5;

    public decimal? TotalStrokesBetPerStroke { get; set; }
    public int? MaxNetScore { get; set; }

    public bool InvestmentOffEnabled { get; set; }
    public decimal InvestmentOffAmount { get; set; }

    public bool RedemptionEnabled { get; set; }
    public decimal RedemptionAmount { get; set; }

    public bool DunnEnabled { get; set; }
    public decimal DunnAmount { get; set; }

    public bool AutoPressEnabled { get; set; }
    public decimal PressAmount { get; set; }
    public int PressDownThreshold { get; set; } = 2;

    public decimal? SkinsBuyIn { get; set; }
    public decimal? SkinsPerSkinAmount { get; set; }

    public decimal ExpenseDeductionPct { get; set; } = 10;
    public int ScoresCountingPerHole { get; set; }

    public string? ConfigJson { get; set; }

    public ICollection<Team> Teams { get; set; } = [];
    public ICollection<BetResult> Results { get; set; } = [];
}
