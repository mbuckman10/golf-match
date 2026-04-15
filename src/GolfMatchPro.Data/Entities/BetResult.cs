namespace GolfMatchPro.Data.Entities;

public class BetResult
{
    public int BetResultId { get; set; }

    public int BetConfigId { get; set; }
    public BetConfiguration BetConfiguration { get; set; } = null!;

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public decimal WinLossAmount { get; set; }
    public decimal NassauFrontResult { get; set; }
    public decimal NassauBackResult { get; set; }
    public decimal Nassau18Result { get; set; }
    public decimal InvestmentResult { get; set; }
    public decimal TotalStrokesResult { get; set; }

    public int? SkinsWon { get; set; }
    public decimal? SkinsAmount { get; set; }
    public decimal? PressResult { get; set; }

    public string? ResultDetailsJson { get; set; }
}
