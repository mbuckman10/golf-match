using GolfMatchPro.Engine.BestBall;
using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Engine.Tests.BestBall;

public class BestBallCalculatorTests
{
    private readonly BestBallCalculator _calc;

    public BestBallCalculatorTests()
    {
        _calc = new BestBallCalculator(new HandicapCalculator(), new NassauCalculator());
    }

    private static BestBallConfig MakeConfig(
        CompetitionType comp = CompetitionType.MatchPlay,
        bool autoPress = false) => new()
    {
        CompetitionType = comp,
        HandicapPercentage = 100,
        NassauFront = 10,
        NassauBack = 10,
        Nassau18 = 10,
        AutoPressEnabled = autoPress,
        PressAmount = 10,
        PressDownThreshold = 2,
        ExpenseDeductionPct = 10,
        HoleHandicapRankings = Enumerable.Range(1, 18).ToArray(),
        HolePars = Enumerable.Repeat(4, 18).ToArray(),
    };

    private static BestBallPlayerData MakePlayer(int id, string name, int ch, int[] gross) => new()
    {
        PlayerId = id,
        PlayerName = name,
        CourseHandicap = ch,
        GrossScores = gross,
    };

    private static BestBallTeamPair MakeTeam(int num, string name, params BestBallPlayerData[] players) => new()
    {
        TeamNumber = num,
        TeamName = name,
        Players = players.ToList(),
    };

    [Fact]
    public void EqualScores_AllHalved()
    {
        var config = MakeConfig();
        var sh = MakeTeam(1, "SH",
            MakePlayer(1, "A1", 10, Enumerable.Repeat(4, 18).ToArray()),
            MakePlayer(2, "A2", 10, Enumerable.Repeat(4, 18).ToArray()));
        var opp = MakeTeam(2, "Opp",
            MakePlayer(3, "B1", 10, Enumerable.Repeat(4, 18).ToArray()),
            MakePlayer(4, "B2", 10, Enumerable.Repeat(4, 18).ToArray()));

        var result = _calc.Calculate(config, sh, [opp]);

        Assert.Single(result.Matchups);
        Assert.Equal(0m, result.Matchups[0].NassauFrontDollars);
        Assert.Equal(0m, result.Matchups[0].NassauBackDollars);
        Assert.Equal(0m, result.Matchups[0].Nassau18Dollars);
        Assert.Equal(0m, result.Matchups[0].TotalAmountSheetHanger);
    }

    [Fact]
    public void SheetHangerSweeps_WinsAll()
    {
        var config = MakeConfig();
        var sh = MakeTeam(1, "SH",
            MakePlayer(1, "A1", 10, Enumerable.Repeat(3, 18).ToArray()),
            MakePlayer(2, "A2", 10, Enumerable.Repeat(3, 18).ToArray()));
        var opp = MakeTeam(2, "Opp",
            MakePlayer(3, "B1", 10, Enumerable.Repeat(5, 18).ToArray()),
            MakePlayer(4, "B2", 10, Enumerable.Repeat(5, 18).ToArray()));

        var result = _calc.Calculate(config, sh, [opp]);

        Assert.Equal(10m, result.Matchups[0].NassauFrontDollars);
        Assert.Equal(10m, result.Matchups[0].NassauBackDollars);
        Assert.Equal(10m, result.Matchups[0].Nassau18Dollars);
        Assert.Equal(30m, result.Matchups[0].TotalAmountSheetHanger);
    }

    [Fact]
    public void BestBall_UsesLowestNetPerHole()
    {
        var config = MakeConfig();
        // SH player 1 has high scores, player 2 has low scores → best ball = player 2
        var sh = MakeTeam(1, "SH",
            MakePlayer(1, "A1", 10, Enumerable.Repeat(7, 18).ToArray()),
            MakePlayer(2, "A2", 10, Enumerable.Repeat(3, 18).ToArray()));
        // Opp both score 4
        var opp = MakeTeam(2, "Opp",
            MakePlayer(3, "B1", 10, Enumerable.Repeat(4, 18).ToArray()),
            MakePlayer(4, "B2", 10, Enumerable.Repeat(4, 18).ToArray()));

        var result = _calc.Calculate(config, sh, [opp]);

        // SH best ball = 3 per hole, Opp best ball = 4 per hole → SH wins all
        Assert.Equal(30m, result.Matchups[0].TotalAmountSheetHanger);
    }

    [Fact]
    public void MultipleOpponents_SeparateMatchups()
    {
        var config = MakeConfig();
        var sh = MakeTeam(1, "SH",
            MakePlayer(1, "A1", 10, Enumerable.Repeat(3, 18).ToArray()),
            MakePlayer(2, "A2", 10, Enumerable.Repeat(3, 18).ToArray()));
        var opp1 = MakeTeam(2, "Opp1",
            MakePlayer(3, "B1", 10, Enumerable.Repeat(5, 18).ToArray()),
            MakePlayer(4, "B2", 10, Enumerable.Repeat(5, 18).ToArray()));
        var opp2 = MakeTeam(3, "Opp2",
            MakePlayer(5, "C1", 10, Enumerable.Repeat(5, 18).ToArray()),
            MakePlayer(6, "C2", 10, Enumerable.Repeat(5, 18).ToArray()));

        var result = _calc.Calculate(config, sh, [opp1, opp2]);

        Assert.Equal(2, result.Matchups.Count);
        Assert.Equal(30m, result.Matchups[0].TotalAmountSheetHanger);
        Assert.Equal(30m, result.Matchups[1].TotalAmountSheetHanger);
    }

    [Fact]
    public void PlayerResults_SplitEvenlyWithinTeam()
    {
        var config = MakeConfig();
        var sh = MakeTeam(1, "SH",
            MakePlayer(1, "A1", 10, Enumerable.Repeat(3, 18).ToArray()),
            MakePlayer(2, "A2", 10, Enumerable.Repeat(3, 18).ToArray()));
        var opp = MakeTeam(2, "Opp",
            MakePlayer(3, "B1", 10, Enumerable.Repeat(5, 18).ToArray()),
            MakePlayer(4, "B2", 10, Enumerable.Repeat(5, 18).ToArray()));

        var result = _calc.Calculate(config, sh, [opp]);

        // SH wins $30 total, split between 2 SH players → $15 each
        var p1 = result.PlayerResults.First(p => p.PlayerId == 1);
        var p2 = result.PlayerResults.First(p => p.PlayerId == 2);
        Assert.Equal(p1.WinLoss, p2.WinLoss);
        Assert.Equal(15m, p1.WinLoss);
        // After 10% expense on winners
        Assert.Equal(13.5m, p1.WinLossAfterExpense);
    }

    [Fact]
    public void EmptyTeam_ReturnsEmpty()
    {
        var config = MakeConfig();
        var sh = new BestBallTeamPair { TeamNumber = 1, Players = [] };
        var opp = MakeTeam(2, "Opp",
            MakePlayer(3, "B1", 10, Enumerable.Repeat(4, 18).ToArray()),
            MakePlayer(4, "B2", 10, Enumerable.Repeat(4, 18).ToArray()));

        var result = _calc.Calculate(config, sh, [opp]);

        Assert.Empty(result.Matchups);
    }

    [Fact]
    public void HandicapDifference_AffectsBestBall()
    {
        var config = MakeConfig();
        // SH players CH=0, Opp players CH=18 → opp gets strokes
        var sh = MakeTeam(1, "SH",
            MakePlayer(1, "A1", 0, Enumerable.Repeat(4, 18).ToArray()),
            MakePlayer(2, "A2", 0, Enumerable.Repeat(4, 18).ToArray()));
        var opp = MakeTeam(2, "Opp",
            MakePlayer(3, "B1", 18, Enumerable.Repeat(5, 18).ToArray()),
            MakePlayer(4, "B2", 18, Enumerable.Repeat(5, 18).ToArray()));

        var result = _calc.Calculate(config, sh, [opp]);

        // Opp gets 18 strokes → net 4 on every hole. SH net = 4 everywhere. Halved.
        Assert.Equal(0m, result.Matchups[0].TotalAmountSheetHanger);
    }
}
