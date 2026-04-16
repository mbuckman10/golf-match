using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Engine.RoundRobin;

namespace GolfMatchPro.Engine.Tests.RoundRobin;

public class RoundRobinCalculatorTests
{
    private readonly RoundRobinCalculator _calculator = new(new NassauCalculator());

    [Fact]
    public void CalculateFoursomeRoundRobin_GeneratesAllPairwiseMatchups()
    {
        var teams = new List<TeamRoundRobinData>
        {
            CreateTeam(1, "Team 1", 4),
            CreateTeam(2, "Team 2", 5),
            CreateTeam(3, "Team 3", 6),
            CreateTeam(4, "Team 4", 7),
        };

        var result = _calculator.CalculateFoursomeRoundRobin(
            teams,
            nassauFront: 5,
            nassauBack: 5,
            nassau18: 5,
            investmentOffEnabled: false,
            investmentOffAmount: 0,
            redemptionEnabled: false,
            redemptionAmount: 0);

        Assert.Equal(6, result.Matchups.Count); // C(4,2) = 6

        var uniquePairs = result.Matchups
            .Select(m => (Math.Min(m.EntityAId, m.EntityBId), Math.Max(m.EntityAId, m.EntityBId)))
            .Distinct()
            .ToList();

        Assert.Equal(6, uniquePairs.Count);

        Assert.Equal(4, result.Leaderboard.Count);
        Assert.All(result.Leaderboard, entry => Assert.Equal(3, entry.MatchupsPlayed)); // Each team plays N-1 opponents
    }

    [Fact]
    public void CalculateIndividualRoundRobin_GeneratesAllPairwiseMatchups()
    {
        var players = new List<PlayerRoundRobinData>
        {
            CreatePlayer(1, "P1", 4),
            CreatePlayer(2, "P2", 5),
            CreatePlayer(3, "P3", 6),
            CreatePlayer(4, "P4", 7),
            CreatePlayer(5, "P5", 8),
        };

        var result = _calculator.CalculateIndividualRoundRobin(
            players,
            nassauFront: 5,
            nassauBack: 5,
            nassau18: 5,
            autoPressPressEnabled: false,
            pressAmount: 0,
            pressDownThreshold: 2);

        Assert.Equal(10, result.Matchups.Count); // C(5,2) = 10

        var uniquePairs = result.Matchups
            .Select(m => (Math.Min(m.EntityAId, m.EntityBId), Math.Max(m.EntityAId, m.EntityBId)))
            .Distinct()
            .ToList();

        Assert.Equal(10, uniquePairs.Count);

        Assert.Equal(5, result.Leaderboard.Count);
        Assert.All(result.Leaderboard, entry => Assert.Equal(4, entry.MatchupsPlayed)); // Each player plays N-1 opponents
    }

    [Fact]
    public void CalculateBestBallRoundRobin_WinLossIsSymmetricPerMatchup()
    {
        var teams = new List<TeamRoundRobinData>
        {
            CreateTeam(1, "A", 3),
            CreateTeam(2, "B", 5),
            CreateTeam(3, "C", 4),
        };

        var result = _calculator.CalculateBestBallRoundRobin(
            teams,
            nassauFront: 5,
            nassauBack: 5,
            nassau18: 5);

        Assert.Equal(3, result.Matchups.Count); // C(3,2) = 3

        foreach (var matchup in result.Matchups)
        {
            Assert.Equal(0m, matchup.EntityAWinLoss + matchup.EntityBWinLoss);
        }
    }

    private static TeamRoundRobinData CreateTeam(int id, string name, int score)
    {
        var p1 = Enumerable.Repeat(score, 18).ToArray();
        var p2 = Enumerable.Repeat(score + 1, 18).ToArray();

        return new TeamRoundRobinData
        {
            TeamId = id,
            TeamName = name,
            ScoresCountingPerHole = 2,
            PlayerNetScores = new List<int[]> { p1, p2 }
        };
    }

    private static PlayerRoundRobinData CreatePlayer(int id, string name, int score)
    {
        return new PlayerRoundRobinData
        {
            PlayerId = id,
            PlayerName = name,
            NetScores = Enumerable.Repeat(score, 18).ToArray()
        };
    }
}
