using GolfMatchPro.Engine.Nassau;

namespace GolfMatchPro.Engine.Tests.Nassau;

public class NassauCalculatorTests
{
    private readonly NassauCalculator _calc = new();

    #region MatchPlay

    [Fact]
    public void MatchPlay_AWinsAllHoles_18Up()
    {
        // A scores 3 every hole, B scores 5 every hole
        var a = Enumerable.Repeat(3, 18).ToArray();
        var b = Enumerable.Repeat(5, 18).ToArray();

        var result = _calc.CalculateMatchPlay(a, b);

        Assert.Equal(9, result.Front9Result);
        Assert.Equal(9, result.Back9Result);
        Assert.Equal(18, result.Overall18Result);
    }

    [Fact]
    public void MatchPlay_Halved()
    {
        var a = Enumerable.Repeat(4, 18).ToArray();
        var b = Enumerable.Repeat(4, 18).ToArray();

        var result = _calc.CalculateMatchPlay(a, b);

        Assert.Equal(0, result.Front9Result);
        Assert.Equal(0, result.Back9Result);
        Assert.Equal(0, result.Overall18Result);
    }

    [Fact]
    public void MatchPlay_BWinsBy2()
    {
        var a = new int[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
        var b = new int[] { 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };

        var result = _calc.CalculateMatchPlay(a, b);

        Assert.Equal(-2, result.Front9Result); // B won holes 1,2
        Assert.Equal(0, result.Back9Result);
        Assert.Equal(-2, result.Overall18Result);
    }

    [Fact]
    public void MatchPlay_SplitFrontBack()
    {
        // A wins front 9 by 1, B wins back 9 by 1
        var a = new int[] { 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 4, 4, 4, 4, 4, 4, 4, 4 };
        var b = new int[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };

        var result = _calc.CalculateMatchPlay(a, b);

        Assert.Equal(1, result.Front9Result);   // A won hole 1
        Assert.Equal(-1, result.Back9Result);   // B won hole 10
        Assert.Equal(0, result.Overall18Result); // Even overall
    }

    [Fact]
    public void MatchPlay_RunningStatus_Tracks()
    {
        var a = new int[] { 3, 5, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
        var b = new int[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };

        var result = _calc.CalculateMatchPlay(a, b);

        Assert.Equal(1, result.HoleByHoleStatus[0]);  // A wins hole 1 → 1 up
        Assert.Equal(0, result.HoleByHoleStatus[1]);   // B wins hole 2 → all square
        Assert.Equal(0, result.HoleByHoleStatus[17]);  // Even at end
    }

    [Fact]
    public void MatchPlay_SkipsUnplayedHoles()
    {
        var a = new int[] { 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        var b = new int[] { 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        var result = _calc.CalculateMatchPlay(a, b);
        Assert.Equal(1, result.HoleByHoleStatus[0]);
        Assert.Equal(1, result.HoleByHoleStatus[2]); // Carries forward
    }

    #endregion

    #region MedalPlay

    [Fact]
    public void MedalPlay_AWinsLowerTotal()
    {
        var a = Enumerable.Repeat(4, 18).ToArray(); // Total: 72
        var b = Enumerable.Repeat(5, 18).ToArray(); // Total: 90

        var result = _calc.CalculateMedalPlay(a, b);

        Assert.True(result.Front9Result > 0);   // A better on front
        Assert.True(result.Back9Result > 0);     // A better on back
        Assert.True(result.Overall18Result > 0); // A better overall
    }

    [Fact]
    public void MedalPlay_Tied()
    {
        var a = Enumerable.Repeat(4, 18).ToArray();
        var b = Enumerable.Repeat(4, 18).ToArray();

        var result = _calc.CalculateMedalPlay(a, b);

        Assert.Equal(0, result.Front9Result);
        Assert.Equal(0, result.Back9Result);
        Assert.Equal(0, result.Overall18Result);
    }

    [Fact]
    public void MedalPlay_FrontBackSplit()
    {
        var a = new int[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 5, 5, 5, 5, 5, 5, 5, 5, 5 }; // Front: 27, Back: 45
        var b = new int[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3 }; // Front: 45, Back: 27

        var result = _calc.CalculateMedalPlay(a, b);

        Assert.True(result.Front9Result > 0);   // A better on front (27 vs 45)
        Assert.True(result.Back9Result < 0);     // B better on back (45 vs 27)
        Assert.Equal(0, result.Overall18Result); // Equal overall (72 vs 72)
    }

    #endregion
}
