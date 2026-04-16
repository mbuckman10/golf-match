using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Engine.Investments;
using GolfMatchPro.Shared.Dtos;

namespace GolfMatchPro.Engine.RoundRobin;

/// <summary>
/// Data structure for a team with player scores for a match.
/// </summary>
public class TeamRoundRobinData
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public List<int[]> PlayerNetScores { get; set; } = []; // Each element is 18-hole net scores
    public List<int[]> PlayerGrossScores { get; set; } = []; // Each element is 18-hole gross scores
    public List<int[]> PlayerHandicapStrokes { get; set; } = []; // Each element is 18-hole strokes received/given
    public int[] HolePars { get; set; } = Enumerable.Repeat(4, 18).ToArray();
    public int ScoresCountingPerHole { get; set; }
}

/// <summary>
/// Data structure for individual player in round robin.
/// </summary>
public class PlayerRoundRobinData
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int[] NetScores { get; set; } = new int[18]; // Net scores for all 18 holes
}

/// <summary>
/// Internal matchup calculation result.
/// </summary>
public class MatchupCalculationResult
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
/// Calculates round robin results where every participant plays every other participant.
/// </summary>
public interface IRoundRobinCalculator
{
    /// <summary>
    /// Calculate foursome round robin (every team vs every other team).
    /// </summary>
    RoundRobinResultDto CalculateFoursomeRoundRobin(
        List<TeamRoundRobinData> teams,
        decimal nassauFront,
        decimal nassauBack,
        decimal nassau18,
        bool investmentOffEnabled,
        decimal investmentOffAmount,
        bool redemptionEnabled,
        decimal redemptionAmount);

    /// <summary>
    /// Calculate individual round robin (every player vs every other player).
    /// </summary>
    RoundRobinResultDto CalculateIndividualRoundRobin(
        List<PlayerRoundRobinData> players,
        decimal nassauFront,
        decimal nassauBack,
        decimal nassau18,
        bool autoPressPressEnabled,
        decimal pressAmount,
        int pressDownThreshold);

    /// <summary>
    /// Calculate best ball round robin (every 2-man team vs every other 2-man team).
    /// </summary>
    RoundRobinResultDto CalculateBestBallRoundRobin(
        List<TeamRoundRobinData> teams,
        decimal nassauFront,
        decimal nassauBack,
        decimal nassau18);
}

public class RoundRobinCalculator : IRoundRobinCalculator
{
    private readonly INassauCalculator _nassauCalculator;

    public RoundRobinCalculator(INassauCalculator nassauCalculator)
    {
        _nassauCalculator = nassauCalculator;
    }

    public RoundRobinResultDto CalculateFoursomeRoundRobin(
        List<TeamRoundRobinData> teams,
        decimal nassauFront,
        decimal nassauBack,
        decimal nassau18,
        bool investmentOffEnabled,
        decimal investmentOffAmount,
        bool redemptionEnabled,
        decimal redemptionAmount)
    {
        var matchups = new List<MatchupCalculationResult>();
        var matchupNumber = 1;

        // Generate all C(n,2) pairwise combinations
        for (int i = 0; i < teams.Count - 1; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                var teamA = teams[i];
                var teamB = teams[j];

                // Compute team hole scores (best N net scores per hole)
                var teamAHoleScores = ComputeTeamHoleScores(teamA.PlayerNetScores, teamA.ScoresCountingPerHole);
                var teamBHoleScores = ComputeTeamHoleScores(teamB.PlayerNetScores, teamB.ScoresCountingPerHole);

                // Calculate Nassau (Medal Play for team bets)
                var nassauResult = _nassauCalculator.CalculateMedalPlay(teamAHoleScores, teamBHoleScores);
                decimal frontDollars = nassauResult.Front9Result > 0 ? nassauFront
                    : nassauResult.Front9Result < 0 ? -nassauFront : 0;
                decimal backDollars = nassauResult.Back9Result > 0 ? nassauBack
                    : nassauResult.Back9Result < 0 ? -nassauBack : 0;
                decimal overallDollars = nassauResult.Overall18Result > 0 ? nassau18
                    : nassauResult.Overall18Result < 0 ? -nassau18 : 0;
                decimal nassauTotalA = frontDollars + backDollars + overallDollars;

                decimal investmentResult = 0;
                if (investmentOffEnabled || redemptionEnabled)
                {
                    investmentResult = CalculateInvestments(
                        teamA, teamB,
                        investmentOffEnabled, investmentOffAmount,
                        redemptionEnabled, redemptionAmount);
                }

                var totalAWinLoss = nassauTotalA + investmentResult;
                var totalBWinLoss = -nassauTotalA - investmentResult;

                matchups.Add(new MatchupCalculationResult
                {
                    Matchup = matchupNumber++,
                    EntityAId = teamA.TeamId,
                    EntityAName = teamA.TeamName,
                    EntityBId = teamB.TeamId,
                    EntityBName = teamB.TeamName,
                    EntityAWinLoss = totalAWinLoss,
                    EntityBWinLoss = totalBWinLoss,
                    ResultDetails = null // TODO: serialize hole-by-hole breakdown
                });
            }
        }

        // Build leaderboard
        var leaderboard = BuildTeamLeaderboard(teams, matchups);

        return new RoundRobinResultDto
        {
            RoundRobinId = 0, // Will be assigned by persistence layer
            Matchups = matchups
                .Select(m => new MatchupResultDto
                {
                    Matchup = m.Matchup,
                    EntityAId = m.EntityAId,
                    EntityAName = m.EntityAName,
                    EntityBId = m.EntityBId,
                    EntityBName = m.EntityBName,
                    EntityAWinLoss = m.EntityAWinLoss,
                    EntityBWinLoss = m.EntityBWinLoss,
                    ResultDetails = m.ResultDetails
                })
                .ToList(),
            Leaderboard = leaderboard
        };
    }

    public RoundRobinResultDto CalculateIndividualRoundRobin(
        List<PlayerRoundRobinData> players,
        decimal nassauFront,
        decimal nassauBack,
        decimal nassau18,
        bool autoPressPressEnabled,
        decimal pressAmount,
        int pressDownThreshold)
    {
        var matchups = new List<MatchupCalculationResult>();
        var matchupNumber = 1;

        // Generate all C(n,2) pairwise combinations
        for (int i = 0; i < players.Count - 1; i++)
        {
            for (int j = i + 1; j < players.Count; j++)
            {
                var playerA = players[i];
                var playerB = players[j];

                // Calculate 1v1 Nassau (Match Play)
                var nassauResult = _nassauCalculator.CalculateMatchPlay(
                    playerA.NetScores, playerB.NetScores);
                decimal frontDollars = nassauResult.Front9Result > 0 ? nassauFront
                    : nassauResult.Front9Result < 0 ? -nassauFront : 0;
                decimal backDollars = nassauResult.Back9Result > 0 ? nassauBack
                    : nassauResult.Back9Result < 0 ? -nassauBack : 0;
                decimal overallDollars = nassauResult.Overall18Result > 0 ? nassau18
                    : nassauResult.Overall18Result < 0 ? -nassau18 : 0;
                decimal nassauTotalA = frontDollars + backDollars + overallDollars;

                var totalAWinLoss = nassauTotalA;
                var totalBWinLoss = -nassauTotalA;

                // TODO: Add auto-press logic if enabled

                matchups.Add(new MatchupCalculationResult
                {
                    Matchup = matchupNumber++,
                    EntityAId = playerA.PlayerId,
                    EntityAName = playerA.PlayerName,
                    EntityBId = playerB.PlayerId,
                    EntityBName = playerB.PlayerName,
                    EntityAWinLoss = totalAWinLoss,
                    EntityBWinLoss = totalBWinLoss,
                    ResultDetails = null
                });
            }
        }

        // Build leaderboard
        var leaderboard = BuildPlayerLeaderboard(players, matchups);

        return new RoundRobinResultDto
        {
            RoundRobinId = 0,
            Matchups = matchups
                .Select(m => new MatchupResultDto
                {
                    Matchup = m.Matchup,
                    EntityAId = m.EntityAId,
                    EntityAName = m.EntityAName,
                    EntityBId = m.EntityBId,
                    EntityBName = m.EntityBName,
                    EntityAWinLoss = m.EntityAWinLoss,
                    EntityBWinLoss = m.EntityBWinLoss,
                    ResultDetails = m.ResultDetails
                })
                .ToList(),
            Leaderboard = leaderboard
        };
    }

    public RoundRobinResultDto CalculateBestBallRoundRobin(
        List<TeamRoundRobinData> teams,
        decimal nassauFront,
        decimal nassauBack,
        decimal nassau18)
    {
        var matchups = new List<MatchupCalculationResult>();
        var matchupNumber = 1;

        // Generate all C(n,2) pairwise combinations
        for (int i = 0; i < teams.Count - 1; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                var teamA = teams[i];
                var teamB = teams[j];

                // Compute best ball scores (best of 2 per hole)
                var teamABestBall = ComputeBestBallScores(teamA.PlayerNetScores);
                var teamBBestBall = ComputeBestBallScores(teamB.PlayerNetScores);

                // Calculate Nassau (Match Play)
                var nassauResult = _nassauCalculator.CalculateMatchPlay(teamABestBall, teamBBestBall);
                decimal frontDollars = nassauResult.Front9Result > 0 ? nassauFront
                    : nassauResult.Front9Result < 0 ? -nassauFront : 0;
                decimal backDollars = nassauResult.Back9Result > 0 ? nassauBack
                    : nassauResult.Back9Result < 0 ? -nassauBack : 0;
                decimal overallDollars = nassauResult.Overall18Result > 0 ? nassau18
                    : nassauResult.Overall18Result < 0 ? -nassau18 : 0;
                decimal nassauTotalA = frontDollars + backDollars + overallDollars;

                matchups.Add(new MatchupCalculationResult
                {
                    Matchup = matchupNumber++,
                    EntityAId = teamA.TeamId,
                    EntityAName = teamA.TeamName,
                    EntityBId = teamB.TeamId,
                    EntityBName = teamB.TeamName,
                    EntityAWinLoss = nassauTotalA,
                    EntityBWinLoss = -nassauTotalA,
                    ResultDetails = null
                });
            }
        }

        // Build leaderboard
        var leaderboard = BuildTeamLeaderboard(teams, matchups);

        return new RoundRobinResultDto
        {
            RoundRobinId = 0,
            Matchups = matchups
                .Select(m => new MatchupResultDto
                {
                    Matchup = m.Matchup,
                    EntityAId = m.EntityAId,
                    EntityAName = m.EntityAName,
                    EntityBId = m.EntityBId,
                    EntityBName = m.EntityBName,
                    EntityAWinLoss = m.EntityAWinLoss,
                    EntityBWinLoss = m.EntityBWinLoss,
                    ResultDetails = m.ResultDetails
                })
                .ToList(),
            Leaderboard = leaderboard
        };
    }

    /// <summary>
    /// Compute team hole scores by taking best N net scores per hole.
    /// </summary>
    private int[] ComputeTeamHoleScores(List<int[]> playerNetScores, int scoresCountingPerHole)
    {
        var teamScores = new int[18];

        for (int h = 0; h < 18; h++)
        {
            var holeScores = playerNetScores
                .Select(p => p[h])
                .OrderBy(s => s)
                .Take(scoresCountingPerHole);
            teamScores[h] = holeScores.Sum();
        }
        return teamScores;
    }

    /// <summary>
    /// Compute best ball scores (best of 2) for each hole.
    /// </summary>
    private int[] ComputeBestBallScores(List<int[]> playerNetScores)
    {
        var bestBall = new int[18];

        for (int h = 0; h < 18; h++)
        {
            bestBall[h] = playerNetScores
                .Select(p => p[h])
                .Min();
        }
        return bestBall;
    }

    /// <summary>
    /// Calculate investment result (offs/redemptions) between two teams.
    /// </summary>
    private decimal CalculateInvestments(
        TeamRoundRobinData teamA,
        TeamRoundRobinData teamB,
        bool investmentOffEnabled,
        decimal investmentOffAmount,
        bool redemptionEnabled,
        decimal redemptionAmount)
    {
        var parsA = teamA.HolePars?.Length == 18 ? teamA.HolePars : Enumerable.Repeat(4, 18).ToArray();
        var parsB = teamB.HolePars?.Length == 18 ? teamB.HolePars : parsA;

        // Fallback for older call sites/tests that may only provide net-like scores.
        var grossA = teamA.PlayerGrossScores.Count > 0 ? teamA.PlayerGrossScores : teamA.PlayerNetScores;
        var grossB = teamB.PlayerGrossScores.Count > 0 ? teamB.PlayerGrossScores : teamB.PlayerNetScores;
        var strokesA = teamA.PlayerHandicapStrokes.Count > 0
            ? teamA.PlayerHandicapStrokes
            : grossA.Select(_ => new int[18]).ToList();
        var strokesB = teamB.PlayerHandicapStrokes.Count > 0
            ? teamB.PlayerHandicapStrokes
            : grossB.Select(_ => new int[18]).ToList();

        var teamAInvestment = InvestmentCalculator.Evaluate(
            grossA,
            parsA,
            strokesA,
            teamA.ScoresCountingPerHole,
            investmentOffAmount,
            redemptionAmount);

        var teamBInvestment = InvestmentCalculator.Evaluate(
            grossB,
            parsB,
            strokesB,
            teamB.ScoresCountingPerHole,
            investmentOffAmount,
            redemptionAmount);

        var teamAAmount = InvestmentCalculator.CalculateAmount(
            teamAInvestment,
            investmentOffAmount,
            redemptionAmount,
            opposingTeamCount: 1,
            offEnabled: investmentOffEnabled,
            redemptionEnabled: redemptionEnabled);

        var teamBAmount = InvestmentCalculator.CalculateAmount(
            teamBInvestment,
            investmentOffAmount,
            redemptionAmount,
            opposingTeamCount: 1,
            offEnabled: investmentOffEnabled,
            redemptionEnabled: redemptionEnabled);

        // Return A-side relative net from investments.
        return teamAAmount - teamBAmount;
    }

    /// <summary>
    /// Build leaderboard for teams.
    /// </summary>
    private List<LeaderboardEntryDto> BuildTeamLeaderboard(
        List<TeamRoundRobinData> teams,
        List<MatchupCalculationResult> matchups)
    {
        var leaderboard = teams
            .Select(t => new LeaderboardEntryDto
            {
                EntityId = t.TeamId,
                EntityName = t.TeamName,
                TotalWinLoss = matchups
                    .Where(m => m.EntityAId == t.TeamId)
                    .Sum(m => m.EntityAWinLoss) +
                    matchups
                    .Where(m => m.EntityBId == t.TeamId)
                    .Sum(m => m.EntityBWinLoss),
                MatchupsPlayed = matchups.Count(m => m.EntityAId == t.TeamId || m.EntityBId == t.TeamId)
            })
            .OrderByDescending(e => e.TotalWinLoss)
            .Select((e, idx) => { e.Rank = idx + 1; return e; })
            .ToList();

        return leaderboard;
    }

    /// <summary>
    /// Build leaderboard for players.
    /// </summary>
    private List<LeaderboardEntryDto> BuildPlayerLeaderboard(
        List<PlayerRoundRobinData> players,
        List<MatchupCalculationResult> matchups)
    {
        var leaderboard = players
            .Select(p => new LeaderboardEntryDto
            {
                EntityId = p.PlayerId,
                EntityName = p.PlayerName,
                TotalWinLoss = matchups
                    .Where(m => m.EntityAId == p.PlayerId)
                    .Sum(m => m.EntityAWinLoss) +
                    matchups
                    .Where(m => m.EntityBId == p.PlayerId)
                    .Sum(m => m.EntityBWinLoss),
                MatchupsPlayed = matchups.Count(m => m.EntityAId == p.PlayerId || m.EntityBId == p.PlayerId)
            })
            .OrderByDescending(e => e.TotalWinLoss)
            .Select((e, idx) => { e.Rank = idx + 1; return e; })
            .ToList();

        return leaderboard;
    }
}
