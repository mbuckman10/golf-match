using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Individual;
using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Engine.Tests.Individual;

public class IndividualBetCalculatorTests
{
    private readonly IndividualBetCalculator _calc;

    public IndividualBetCalculatorTests()
    {
        _calc = new IndividualBetCalculator(new HandicapCalculator(), new NassauCalculator());
    }

    private static IndividualBetConfig MakeConfig(
        CompetitionType comp = CompetitionType.MatchPlay,
        bool autoPress = false, int pressThreshold = 2) => new()
    {
        CompetitionType = comp,
        HandicapPercentage = 100,
        NassauFront = 5,
        NassauBack = 5,
        Nassau18 = 5,
        AutoPressEnabled = autoPress,
        PressAmount = 5,
        PressDownThreshold = pressThreshold,
        ExpenseDeductionPct = 10,
        HoleHandicapRankings = Enumerable.Range(1, 18).ToArray(),
        HolePars = Enumerable.Repeat(4, 18).ToArray(),
    };

    private static IndividualPlayerData MakePlayer(int id, string name, int ch, int[] gross) => new()
    {
        PlayerId = id,
        PlayerName = name,
        CourseHandicap = ch,
        GrossScores = gross,
    };

    [Fact]
    public void EqualScores_NoWinLoss()
    {
        var config = MakeConfig();
        var players = new List<IndividualPlayerData>
        {
            MakePlayer(1, "Alice", 10, Enumerable.Repeat(5, 18).ToArray()),
            MakePlayer(2, "Bob", 10, Enumerable.Repeat(5, 18).ToArray()),
        };
        var matchups = new List<IndividualMatchup> { new() { PlayerAId = 1, PlayerBId = 2 } };

        var result = _calc.Calculate(config, players, matchups);

        Assert.Single(result.Matchups);
        Assert.Equal(0m, result.Matchups[0].NassauFrontDollars);
        Assert.Equal(0m, result.Matchups[0].NassauBackDollars);
        Assert.Equal(0m, result.Matchups[0].Nassau18Dollars);
        Assert.Equal(0m, result.PlayerResults.First(p => p.PlayerId == 1).WinLoss);
    }

    [Fact]
    public void PlayerA_Sweeps_MatchPlay()
    {
        var config = MakeConfig();
        // Both same handicap, A scores lower on every hole
        var players = new List<IndividualPlayerData>
        {
            MakePlayer(1, "Alice", 10, Enumerable.Repeat(3, 18).ToArray()),
            MakePlayer(2, "Bob", 10, Enumerable.Repeat(5, 18).ToArray()),
        };
        var matchups = new List<IndividualMatchup> { new() { PlayerAId = 1, PlayerBId = 2 } };

        var result = _calc.Calculate(config, players, matchups);

        Assert.Equal(5m, result.Matchups[0].NassauFrontDollars);
        Assert.Equal(5m, result.Matchups[0].NassauBackDollars);
        Assert.Equal(5m, result.Matchups[0].Nassau18Dollars);
        Assert.Equal(15m, result.Matchups[0].TotalAmountPlayerA);

        // Expense deduction: winner gets 10% deducted
        var aliceResult = result.PlayerResults.First(p => p.PlayerId == 1);
        Assert.Equal(15m, aliceResult.WinLoss);
        Assert.Equal(13.5m, aliceResult.WinLossAfterExpense); // 15 * 0.9

        var bobResult = result.PlayerResults.First(p => p.PlayerId == 2);
        Assert.Equal(-15m, bobResult.WinLoss);
        Assert.Equal(-15m, bobResult.WinLossAfterExpense); // losers not deducted
    }

    [Fact]
    public void HandicapDifference_Adjusts_NetScores()
    {
        var config = MakeConfig();
        // Player A: CH 10, Player B: CH 20 → B gets 10 extra strokes
        var grossA = Enumerable.Repeat(4, 18).ToArray();
        var grossB = Enumerable.Repeat(5, 18).ToArray();

        var players = new List<IndividualPlayerData>
        {
            MakePlayer(1, "Alice", 10, grossA),
            MakePlayer(2, "Bob", 20, grossB),
        };
        var matchups = new List<IndividualMatchup> { new() { PlayerAId = 1, PlayerBId = 2 } };

        var result = _calc.Calculate(config, players, matchups);

        // Bob gets 10 strokes off (on holes ranked 1-10), making his net 4 on those holes
        // and 5 on holes 11-18. Alice net = 4 everywhere.
        // So on holes 1-10, tie. On holes 11-18, Alice wins.
        // Front 9: holes 1-9 → holes ranked 1-9 get strokes for Bob, tie on 1-9 → front halved
        // Actually: with HoleHandicapRankings = [1..18], holes 1-10 get strokes
        // Front 9 (holes 1-9): Bob gets stroke on all 9 → net 4 vs 4 = halved
        // Back 9 (holes 10-18): Bob gets stroke on hole 10 only → hole 10: net 4 vs 4, holes 11-18: net 5 vs 4
        // Back 9 Alice wins 8 holes
        // Overall: Alice dominates back 9
        Assert.Equal(0m, result.Matchups[0].NassauFrontDollars);
        Assert.True(result.Matchups[0].NassauBackDollars > 0); // Alice wins back
    }

    [Fact]
    public void MedalPlay_Works()
    {
        var config = MakeConfig(CompetitionType.MedalPlay);
        var players = new List<IndividualPlayerData>
        {
            MakePlayer(1, "Alice", 10, Enumerable.Repeat(4, 18).ToArray()),
            MakePlayer(2, "Bob", 10, Enumerable.Repeat(5, 18).ToArray()),
        };
        var matchups = new List<IndividualMatchup> { new() { PlayerAId = 1, PlayerBId = 2 } };

        var result = _calc.Calculate(config, players, matchups);

        // Alice has lower total on both nines and overall
        Assert.Equal(5m, result.Matchups[0].NassauFrontDollars);
        Assert.Equal(5m, result.Matchups[0].NassauBackDollars);
        Assert.Equal(5m, result.Matchups[0].Nassau18Dollars);
    }

    [Fact]
    public void MultipleMatchups_AccumulatesCorrectly()
    {
        var config = MakeConfig();
        var players = new List<IndividualPlayerData>
        {
            MakePlayer(1, "A", 10, Enumerable.Repeat(3, 18).ToArray()),
            MakePlayer(2, "B", 10, Enumerable.Repeat(5, 18).ToArray()),
            MakePlayer(3, "C", 10, Enumerable.Repeat(5, 18).ToArray()),
        };
        var matchups = new List<IndividualMatchup>
        {
            new() { PlayerAId = 1, PlayerBId = 2 },
            new() { PlayerAId = 1, PlayerBId = 3 },
        };

        var result = _calc.Calculate(config, players, matchups);

        Assert.Equal(2, result.Matchups.Count);
        // Player 1 wins both matchups → $15 each = $30 total
        var p1 = result.PlayerResults.First(p => p.PlayerId == 1);
        Assert.Equal(30m, p1.WinLoss);
    }

    [Fact]
    public void EmptyPlayersOrMatchups_ReturnsEmpty()
    {
        var config = MakeConfig();

        var r1 = _calc.Calculate(config, new List<IndividualPlayerData>(), []);
        Assert.Empty(r1.Matchups);

        var r2 = _calc.Calculate(config,
            [MakePlayer(1, "A", 10, Enumerable.Repeat(4, 18).ToArray())],
            []);
        Assert.Empty(r2.Matchups);
    }

    #region AutoPress

    [Fact]
    public void AutoPress_TriggersWhenNDown()
    {
        // Player A loses first 2 holes then ties the rest → press triggers at hole 2
        var status = new int[18];
        status[0] = -1; // down 1
        status[1] = -2; // down 2 → triggers press
        for (int h = 2; h < 18; h++) status[h] = -2; // stays at -2

        var presses = IndividualBetCalculator.CalculateAutoPresses(status, 2, 5m);

        Assert.NotEmpty(presses);
        Assert.Equal(2, presses[0].StartHole); // 1-based hole 2
    }

    [Fact]
    public void AutoPress_NoPressWhenNotDown()
    {
        var status = Enumerable.Repeat(0, 18).ToArray();

        var presses = IndividualBetCalculator.CalculateAutoPresses(status, 2, 5m);

        Assert.Empty(presses);
    }

    [Fact]
    public void AutoPress_IntegrationTest()
    {
        var config = MakeConfig(autoPress: true, pressThreshold: 2);

        // A scores 6 on holes 1-2 (loses them), 4 on rest. B scores 4 on all.
        var grossA = Enumerable.Repeat(4, 18).ToArray();
        grossA[0] = 6;
        grossA[1] = 6;
        var grossB = Enumerable.Repeat(4, 18).ToArray();

        var players = new List<IndividualPlayerData>
        {
            MakePlayer(1, "Alice", 10, grossA),
            MakePlayer(2, "Bob", 10, grossB),
        };
        var matchups = new List<IndividualMatchup> { new() { PlayerAId = 1, PlayerBId = 2 } };

        var result = _calc.Calculate(config, players, matchups);

        // A should be 2-down after hole 2, triggering a press
        Assert.NotEmpty(result.Matchups[0].Presses);
    }

    #endregion
}
