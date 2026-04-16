using GolfMatchPro.Engine.GrandTotals;
using GolfMatchPro.Shared.Dtos;

namespace GolfMatchPro.Engine.Tests.GrandTotals;

public class GrandTotalCalculatorTests
{
    private readonly GrandTotalCalculator _calculator = new();

    [Fact]
    public void Calculate_IncludesOnlyEnabledBetTypes()
    {
        var amounts = new Dictionary<string, decimal>
        {
            ["Foursomes"] = 10,
            ["Threesomes"] = -5,
            ["Fivesomes"] = 0,
            ["Individual"] = 20,
            ["BestBall"] = -10,
            ["SkinsGross"] = 4,
            ["SkinsNet"] = 1,
            ["Tournament"] = 8,
            ["RoundRobin"] = -3
        };

        var filters = new CalculateGrandTotalsRequest
        {
            IncludeFoursomes = true,
            IncludeThreesomes = false,
            IncludeFivesomes = false,
            IncludeIndividual = true,
            IncludeBestBall = false,
            IncludeSkinsGross = true,
            IncludeSkinsNet = false,
            IncludeIndoTourney = true,
            IncludeRoundRobins = false
        };

        var result = _calculator.Calculate(1, "Player 1", amounts, filters);

        Assert.Equal(10m, result.FoursomesWinLoss);
        Assert.Equal(0m, result.ThreesomesWinLoss);
        Assert.Equal(20m, result.IndividualWinLoss);
        Assert.Equal(4m, result.SkinsGrossWinLoss);
        Assert.Equal(8m, result.IndoTourneyWinLoss);
        Assert.Equal(0m, result.RoundRobinWinLoss);
        Assert.Equal(42m, result.TotalWinLoss);
        Assert.Equal("Win", result.Status);
    }

    [Fact]
    public void Calculate_StatusBreakEven_WhenTotalIsZero()
    {
        var amounts = new Dictionary<string, decimal>
        {
            ["Foursomes"] = 10,
            ["Threesomes"] = -10,
            ["Fivesomes"] = 0,
            ["Individual"] = 0,
            ["BestBall"] = 0,
            ["SkinsGross"] = 0,
            ["SkinsNet"] = 0,
            ["Tournament"] = 0,
            ["RoundRobin"] = 0
        };

        var filters = new CalculateGrandTotalsRequest();
        var result = _calculator.Calculate(2, "Player 2", amounts, filters);

        Assert.Equal(0m, result.TotalWinLoss);
        Assert.Equal("Break Even", result.Status);
    }

    [Fact]
    public void CalculateAllPlayers_SortsByTotalWinLossDescending()
    {
        var players = new List<PlayerGrandTotalDto>
        {
            new() { PlayerId = 1, PlayerName = "A", TotalWinLoss = 5 },
            new() { PlayerId = 2, PlayerName = "B", TotalWinLoss = 25 },
            new() { PlayerId = 3, PlayerName = "C", TotalWinLoss = -3 },
        };

        var result = _calculator.CalculateAllPlayers(
            matchId: 100,
            players,
            new CalculateGrandTotalsRequest());

        Assert.Equal(100, result.MatchId);
        Assert.Equal(3, result.PlayerTotals.Count);
        Assert.Equal(2, result.PlayerTotals[0].PlayerId);
        Assert.Equal(1, result.PlayerTotals[1].PlayerId);
        Assert.Equal(3, result.PlayerTotals[2].PlayerId);
    }
}
