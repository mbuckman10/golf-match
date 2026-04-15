using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Skins;

namespace GolfMatchPro.Engine.Tests.Skins;

public class SkinsCalculatorTests
{
    private readonly SkinsCalculator _calc = new(new HandicapCalculator());

    [Fact]
    public void GrossSkins_CarryOver_AwardsCombinedSkin()
    {
        var config = new SkinsConfig
        {
            UseNetScores = false,
            BuyInPerPlayer = 10m,
        };

        var p1Scores = Enumerable.Repeat(4, 18).ToArray();
        var p2Scores = Enumerable.Repeat(4, 18).ToArray();
        var p3Scores = Enumerable.Repeat(4, 18).ToArray();

        // Hole 1 tie (all 4), Hole 2 unique low by P1 (3)
        p1Scores[1] = 3;

        var players = new List<SkinsPlayerData>
        {
            new() { PlayerId = 1, PlayerName = "P1", GrossScores = p1Scores },
            new() { PlayerId = 2, PlayerName = "P2", GrossScores = p2Scores },
            new() { PlayerId = 3, PlayerName = "P3", GrossScores = p3Scores },
        };

        var result = _calc.Calculate(config, players);

        Assert.Equal(2, result.HoleResults[1].SkinsAwarded); // carry + current
        Assert.Equal(2, result.PlayerResults.First(p => p.PlayerId == 1).SkinsWon);
        Assert.Equal(18, result.TotalSkinsAwarded + result.UnresolvedCarrySkins);
    }

    [Fact]
    public void NetSkins_UsesHandicapStrokes()
    {
        var config = new SkinsConfig
        {
            UseNetScores = true,
            HandicapPercentage = 100m,
            HoleHandicapRankings = Enumerable.Range(1, 18).ToArray(),
            AmountPerSkin = 5m,
        };

        // P1 gross 5 on all holes, CH 0
        // P2 gross 5 on all holes, CH 18 -> gets one stroke each hole, net 4
        var players = new List<SkinsPlayerData>
        {
            new() { PlayerId = 1, PlayerName = "Scratch", CourseHandicap = 0, GrossScores = Enumerable.Repeat(5, 18).ToArray() },
            new() { PlayerId = 2, PlayerName = "Bogey", CourseHandicap = 18, GrossScores = Enumerable.Repeat(5, 18).ToArray() },
        };

        var result = _calc.Calculate(config, players);

        var p2 = result.PlayerResults.First(p => p.PlayerId == 2);
        Assert.Equal(18, p2.SkinsWon);
        Assert.Equal(90m, p2.GrossWinnings);
        Assert.Equal(0, result.UnresolvedCarrySkins);
    }

    [Fact]
    public void BuyInPot_SplitsByAwardedSkins()
    {
        var config = new SkinsConfig
        {
            UseNetScores = false,
            BuyInPerPlayer = 20m,
        };

        var players = new List<SkinsPlayerData>
        {
            new() { PlayerId = 1, PlayerName = "A", GrossScores = Enumerable.Repeat(3, 18).ToArray() },
            new() { PlayerId = 2, PlayerName = "B", GrossScores = Enumerable.Repeat(4, 18).ToArray() },
            new() { PlayerId = 3, PlayerName = "C", GrossScores = Enumerable.Repeat(5, 18).ToArray() },
        };

        var result = _calc.Calculate(config, players);

        Assert.Equal(60m, result.TotalPot);
        Assert.Equal(18, result.TotalSkinsAwarded);
        Assert.Equal(60m / 18m, result.AmountPerAwardedSkin, 6);

        var a = result.PlayerResults.First(p => p.PlayerId == 1);
        Assert.Equal(18, a.SkinsWon);
        Assert.Equal(60m, a.GrossWinnings, 6);
        Assert.Equal(40m, a.NetWinnings, 6); // 60 - 20 buy-in
    }
}
