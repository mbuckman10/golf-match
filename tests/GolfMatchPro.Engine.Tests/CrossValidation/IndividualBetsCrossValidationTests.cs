using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Individual;
using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Engine.Tests.CrossValidation;

/// <summary>
/// Cross-validation for Individual Bets from the Excel template.
/// Match 1: Jack Watkins (hdcp 23) vs JD Moss (hdcp 10)
/// Config: Match Play, Front $5, Back $10, 18-hole $5
/// Auto-press: 2-down, $25 per press (but result shows 0 presses)
/// </summary>
public class IndividualBetsCrossValidationTests
{
    private static readonly int[] Pars = [5, 3, 4, 4, 3, 5, 4, 4, 4, 4, 5, 4, 3, 4, 3, 5, 4, 4];
    private static readonly int[] HdcpRankings = [17, 13, 7, 1, 11, 15, 5, 9, 3, 6, 16, 12, 18, 2, 8, 14, 4, 10];

    // Jack Watkins: course hdcp 23
    private static readonly int[] JackGross = [8, 3, 4, 6, 6, 8, 5, 6, 6, 7, 7, 6, 4, 5, 3, 6, 5, 5];

    // JD Moss: course hdcp 10
    private static readonly int[] JDGross = [6, 3, 5, 5, 4, 8, 5, 5, 7, 4, 5, 4, 3, 4, 3, 8, 4, 5];

    private readonly HandicapCalculator _handicapCalc = new();
    private readonly NassauCalculator _nassauCalc = new();

    [Fact]
    public void JackGetsThirteenStrokes()
    {
        // In the Excel, "shots for this bet" = 13 (Jack 23 - JD 10)
        int jackPlaying = _handicapCalc.ComputePlayingHandicap(23, 1.0m, 10);
        int jdPlaying = _handicapCalc.ComputePlayingHandicap(10, 1.0m, 10);

        Assert.Equal(13, jackPlaying);
        Assert.Equal(0, jdPlaying);
    }

    [Fact]
    public void JackNetScores_MatchExcel()
    {
        // Excel row 19: Jack's net scores with 13 strokes
        int[] strokes = _handicapCalc.DistributeStrokes(13, HdcpRankings);
        int[] jackNet = JackGross.Zip(strokes, (g, s) => g - s).ToArray();

        int[] excelJackNet = [8, 2, 3, 5, 5, 8, 4, 5, 5, 6, 7, 5, 4, 4, 2, 6, 4, 4];
        Assert.Equal(excelJackNet, jackNet);
        Assert.Equal(45, jackNet[..9].Sum());  // Front = 45
        Assert.Equal(42, jackNet[9..].Sum());  // Back = 42
        Assert.Equal(87, jackNet.Sum());       // Total = 87
    }

    [Fact]
    public void JDNetScores_AreGross()
    {
        // JD gets 0 strokes (he's the lower handicap), so net = gross
        int[] strokes = _handicapCalc.DistributeStrokes(0, HdcpRankings);
        Assert.All(strokes, s => Assert.Equal(0, s));
        Assert.Equal(48, JDGross[..9].Sum());  // Front = 48
        Assert.Equal(40, JDGross[9..].Sum());  // Back = 40
        Assert.Equal(88, JDGross.Sum());       // Total = 88
    }

    [Fact]
    public void MatchPlay_HoleByHole_MatchesExcel()
    {
        // Excel row 20: match play status (Jack up/down) per hole
        // Front: -1, 0, 1, 1, 0, 0, 1, 1, 2
        // Back (independent from 0): -1, -2, -3, -4, -4, -3, -2, -2, -1
        int[] jackNet = GetJackNet();

        // The engine's CalculateMatchPlay tracks front/back independently
        NassauResult result = _nassauCalc.CalculateMatchPlay(jackNet, JDGross);

        // Front 9: Jack 2-up
        Assert.Equal(2, result.Front9Result);

        // Back 9: JD 1-up (Jack -1)
        Assert.Equal(-1, result.Back9Result);

        // Overall 18: Jack 1-up
        Assert.Equal(1, result.Overall18Result);
    }

    [Fact]
    public void MatchPlay_FrontBack18_HoleByHole_Detailed()
    {
        int[] jackNet = GetJackNet();
        NassauResult result = _nassauCalc.CalculateMatchPlay(jackNet, JDGross);

        // Verify the running status matches Excel's sequence
        // Front 9 (holes 1-9): -1, 0, +1, +1, 0, 0, +1, +1, +2
        int[] excelFrontStatus = [-1, 0, 1, 1, 0, 0, 1, 1, 2];
        for (int h = 0; h < 9; h++)
            Assert.Equal(excelFrontStatus[h], result.HoleByHoleStatus[h]);

        // The overall running status continues across both nines
        // After hole 9: +2, then back nine holes bring it to +1
        Assert.Equal(1, result.HoleByHoleStatus[17]); // Overall at hole 18
    }

    [Fact]
    public void MatchPlay_Dollars_MatchExcel()
    {
        // Excel: Front=$5 (Jack wins), Back=-$10 (JD wins), 18=$5 (Jack wins), Net=$0
        int[] jackNet = GetJackNet();
        NassauResult result = _nassauCalc.CalculateMatchPlay(jackNet, JDGross);

        decimal frontDollars = result.Front9Result > 0 ? 5m : result.Front9Result < 0 ? -5m : 0;
        decimal backDollars = result.Back9Result > 0 ? 10m : result.Back9Result < 0 ? -10m : 0;
        decimal overallDollars = result.Overall18Result > 0 ? 5m : result.Overall18Result < 0 ? -5m : 0;

        Assert.Equal(5m, frontDollars);
        Assert.Equal(-10m, backDollars);
        Assert.Equal(5m, overallDollars);
        Assert.Equal(0m, frontDollars + backDollars + overallDollars);
    }

    [Fact]
    public void IndividualBetCalculator_FullIntegration_MatchesExcel()
    {
        // Run the full calculator and verify end-to-end
        var config = new IndividualBetConfig
        {
            CompetitionType = CompetitionType.MatchPlay,
            HandicapPercentage = 100,
            NassauFront = 5,
            NassauBack = 10,
            Nassau18 = 5,
            AutoPressEnabled = false, // Excel shows 0 presses
            PressAmount = 25,
            PressDownThreshold = 2,
            ExpenseDeductionPct = 0,
            HoleHandicapRankings = HdcpRankings,
            HolePars = Pars
        };

        var players = new List<IndividualPlayerData>
        {
            new() { PlayerId = 20, PlayerName = "Jack Watkins", CourseHandicap = 23, GrossScores = JackGross },
            new() { PlayerId = 19, PlayerName = "JD Moss", CourseHandicap = 10, GrossScores = JDGross },
        };

        var matchups = new List<IndividualMatchup>
        {
            new() { PlayerAId = 20, PlayerBId = 19 }
        };

        var calculator = new IndividualBetCalculator(_handicapCalc, _nassauCalc);
        var results = calculator.Calculate(config, players, matchups);

        Assert.Single(results.Matchups);
        var match = results.Matchups[0];

        // Jack wins front (2-up) → +$5
        Assert.Equal(5m, match.NassauFrontDollars);
        // JD wins back (1-up) → Jack loses → -$10
        Assert.Equal(-10m, match.NassauBackDollars);
        // Jack wins 18 (1-up) → +$5
        Assert.Equal(5m, match.Nassau18Dollars);
        // Total: $0 — "Jack pushes $0 and JD pushes $0"
        Assert.Equal(0m, match.TotalAmountPlayerA);

        // No presses
        Assert.Empty(match.Presses);

        // Player results
        var jackResult = results.PlayerResults.First(p => p.PlayerId == 20);
        var jdResult = results.PlayerResults.First(p => p.PlayerId == 19);
        Assert.Equal(0m, jackResult.WinLoss);
        Assert.Equal(0m, jdResult.WinLoss);
    }

    private int[] GetJackNet()
    {
        int[] strokes = _handicapCalc.DistributeStrokes(13, HdcpRankings);
        return JackGross.Zip(strokes, (g, s) => g - s).ToArray();
    }
}
