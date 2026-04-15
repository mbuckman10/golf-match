using GolfMatchPro.Engine.Handicaps;

namespace GolfMatchPro.Engine.Tests.Handicaps;

public class HandicapCalculatorTests
{
    private readonly HandicapCalculator _calc = new();

    // Standard ranking: holes ranked 1 through 18 in order
    private static readonly int[] StandardRankings = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18];

    #region ComputeCourseHandicap

    [Theory]
    [InlineData(10.4, 140, 13)]   // 10.4 * 140/113 = 12.88 → 13
    [InlineData(0, 130, 0)]
    [InlineData(20.0, 113, 20)]   // 20 * 113/113 = 20
    [InlineData(10.0, 130, 12)]   // 10 * 130/113 = 11.50 → 12 (AwayFromZero)
    [InlineData(-1.0, 130, -1)]   // -1 * 130/113 = -1.15 → -1
    [InlineData(36.0, 155, 49)]   // 36 * 155/113 = 49.38 → 49
    public void ComputeCourseHandicap_ReturnsExpected(decimal index, int slope, int expected)
    {
        Assert.Equal(expected, _calc.ComputeCourseHandicap(index, slope));
    }

    #endregion

    #region ComputePlayingHandicap

    [Fact]
    public void ComputePlayingHandicap_100Percent_SubtractsLowest()
    {
        // Player has CH 15, lowest in group is 5 → playing 10
        Assert.Equal(10, _calc.ComputePlayingHandicap(15, 1.0m, 5));
    }

    [Fact]
    public void ComputePlayingHandicap_75Percent()
    {
        // Player CH 20 * 0.75 = 15, lowest CH 4 * 0.75 = 3 → 12
        Assert.Equal(12, _calc.ComputePlayingHandicap(20, 0.75m, 4));
    }

    [Fact]
    public void ComputePlayingHandicap_LowestPlayer_GetsZero()
    {
        Assert.Equal(0, _calc.ComputePlayingHandicap(5, 1.0m, 5));
    }

    #endregion

    #region DistributeStrokes

    [Fact]
    public void DistributeStrokes_Handicap10_StrokesOnHardest10()
    {
        var strokes = _calc.DistributeStrokes(10, StandardRankings);
        // Holes ranked 1-10 get 1 stroke each, 11-18 get 0
        for (int i = 0; i < 10; i++)
            Assert.Equal(1, strokes[i]);
        for (int i = 10; i < 18; i++)
            Assert.Equal(0, strokes[i]);
    }

    [Fact]
    public void DistributeStrokes_Handicap22_DoubleStrokesOnHardest4()
    {
        var strokes = _calc.DistributeStrokes(22, StandardRankings);
        // 22 = 18 + 4 → 1 full pass + 4 remainder
        // Holes ranked 1-4: 2 strokes, holes 5-18: 1 stroke
        for (int i = 0; i < 4; i++)
            Assert.Equal(2, strokes[i]);
        for (int i = 4; i < 18; i++)
            Assert.Equal(1, strokes[i]);
    }

    [Fact]
    public void DistributeStrokes_Handicap0_NoStrokes()
    {
        var strokes = _calc.DistributeStrokes(0, StandardRankings);
        Assert.All(strokes, s => Assert.Equal(0, s));
    }

    [Fact]
    public void DistributeStrokes_Handicap18_OneStrokeEverywhere()
    {
        var strokes = _calc.DistributeStrokes(18, StandardRankings);
        Assert.All(strokes, s => Assert.Equal(1, s));
    }

    [Fact]
    public void DistributeStrokes_NegativeHandicap_GivesBackOnEasiest()
    {
        // Handicap -2 → give back strokes on easiest 2 holes (ranked 17, 18)
        var strokes = _calc.DistributeStrokes(-2, StandardRankings);
        for (int i = 0; i < 16; i++)
            Assert.Equal(0, strokes[i]);
        Assert.Equal(-1, strokes[16]); // ranking 17
        Assert.Equal(-1, strokes[17]); // ranking 18
    }

    [Fact]
    public void DistributeStrokes_ShuffledRankings_CorrectDistribution()
    {
        // Rankings not in order: hole 0 is ranked 18 (easiest), hole 17 is ranked 1 (hardest)
        int[] shuffled = [18, 16, 14, 12, 10, 8, 6, 4, 2, 17, 15, 13, 11, 9, 7, 5, 3, 1];
        var strokes = _calc.DistributeStrokes(5, shuffled);

        // Only holes with ranking <= 5 should get strokes
        for (int i = 0; i < 18; i++)
        {
            int expected = shuffled[i] <= 5 ? 1 : 0;
            Assert.Equal(expected, strokes[i]);
        }
    }

    #endregion

    #region ApplyESC

    [Theory]
    [InlineData(9, 4, 5, 7)]    // CH 5, gross 9 on par 4 → cap 7
    [InlineData(8, 4, 3, 6)]    // CH 3, gross 8 on par 4 → cap double bogey (6)
    [InlineData(12, 5, 40, 10)] // CH 40, gross 12 on par 5 → cap 10
    [InlineData(5, 4, 3, 5)]    // CH 3, gross 5 on par 4 → under cap, no change
    [InlineData(4, 4, 15, 4)]   // CH 15, gross 4 on par 4 → under cap, no change
    [InlineData(10, 4, 22, 8)]  // CH 22, gross 10 on par 4 → cap 8
    [InlineData(11, 5, 32, 9)]  // CH 32, gross 11 on par 5 → cap 9
    public void ApplyESC_CapsCorrectly(int gross, int par, int courseHandicap, int expected)
    {
        Assert.Equal(expected, _calc.ApplyESC(gross, par, courseHandicap));
    }

    #endregion

    #region ComputeReportableScore

    [Fact]
    public void ComputeReportableScore_AppliesESCToEachHole()
    {
        int[] grossScores = [5, 4, 9, 4, 5, 4, 3, 5, 4, 5, 4, 3, 4, 5, 4, 5, 4, 3]; // 80 total
        int[] pars =        [4, 4, 4, 3, 5, 4, 3, 5, 4, 4, 4, 3, 4, 5, 4, 5, 4, 3];  // 72 par

        // CH 5 → ESC max is 7. Hole 3 has gross 9 → capped to 7
        int result = _calc.ComputeReportableScore(grossScores, pars, 5);
        // 80 - 2 (9 capped to 7) = 78
        Assert.Equal(78, result);
    }

    [Fact]
    public void ComputeReportableScore_SkipsZeroScores()
    {
        int[] grossScores = [5, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        int[] pars =        [4, 4, 4, 3, 5, 4, 3, 5, 4, 4, 4, 3, 4, 5, 4, 5, 4, 3];

        int result = _calc.ComputeReportableScore(grossScores, pars, 10);
        Assert.Equal(9, result); // 5 + 4
    }

    #endregion

    #region ComputeNetScore

    [Theory]
    [InlineData(85, 13, 72)]
    [InlineData(72, 0, 72)]
    [InlineData(70, -2, 72)]
    public void ComputeNetScore_SubtractsHandicap(int gross, int ch, int expected)
    {
        Assert.Equal(expected, _calc.ComputeNetScore(gross, ch));
    }

    #endregion
}
