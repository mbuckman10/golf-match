using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Shared.Dtos;

public class BetConfigurationDto
{
    public int BetConfigId { get; set; }
    public int MatchId { get; set; }
    public BetType BetType { get; set; }
    public CompetitionType CompetitionType { get; set; }
    public decimal HandicapPercentage { get; set; }
    public decimal NassauFront { get; set; }
    public decimal NassauBack { get; set; }
    public decimal Nassau18 { get; set; }
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
    public int PressDownThreshold { get; set; }
    public decimal? SkinsBuyIn { get; set; }
    public decimal? SkinsPerSkinAmount { get; set; }
    public decimal ExpenseDeductionPct { get; set; }
    public int ScoresCountingPerHole { get; set; }
    public string? ConfigJson { get; set; }
    public List<TeamDto> Teams { get; set; } = [];
}

public class CreateBetConfigurationRequest
{
    public BetType BetType { get; set; }
    public CompetitionType CompetitionType { get; set; }
    public decimal HandicapPercentage { get; set; } = 100;
    public decimal NassauFront { get; set; } = 5;
    public decimal NassauBack { get; set; } = 5;
    public decimal Nassau18 { get; set; } = 5;
    public decimal? TotalStrokesBetPerStroke { get; set; }
    public int? MaxNetScore { get; set; }
    public bool InvestmentOffEnabled { get; set; }
    public decimal InvestmentOffAmount { get; set; } = 6;
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
}

public class TeamDto
{
    public int TeamId { get; set; }
    public int TeamNumber { get; set; }
    public string? TeamName { get; set; }
    public List<TeamPlayerDto> Players { get; set; } = [];
}

public class TeamPlayerDto
{
    public int TeamPlayerId { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public TeamPosition Position { get; set; }
}

public class CreateTeamRequest
{
    public int TeamNumber { get; set; }
    public string? TeamName { get; set; }
    public List<CreateTeamPlayerRequest> Players { get; set; } = [];
}

public class CreateTeamPlayerRequest
{
    public int PlayerId { get; set; }
    public TeamPosition Position { get; set; }
}

public class BetResultDto
{
    public int BetResultId { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public decimal WinLossAmount { get; set; }
    public decimal NassauFrontResult { get; set; }
    public decimal NassauBackResult { get; set; }
    public decimal Nassau18Result { get; set; }
    public decimal InvestmentResult { get; set; }
    public decimal TotalStrokesResult { get; set; }
    public int? SkinsWon { get; set; }
    public decimal? SkinsAmount { get; set; }
    public decimal? PressResult { get; set; }
}
