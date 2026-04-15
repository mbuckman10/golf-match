using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Tournament;

namespace GolfMatchPro.Engine.Tests.Tournament;

public class TournamentCalculatorTests
{
    private readonly TournamentCalculator _calc = new(new HandicapCalculator());

    private static TournamentConfig MakeConfig() => new()
    {
        SponsorMoney = 100m,
        BuyInPerPlayer = 20m,
        ExpenseDeductionPct = 10m,
        HandicapPercentage = 100m,
        HoleHandicapRankings = Enumerable.Range(1, 18).ToArray(),
        GrossPursePercent = 50m,
        NetPursePercent = 50m,
        EighteenHolePercent = 60m,
        FrontNinePercent = 20m,
        BackNinePercent = 20m,
        PlacePayouts =
        [
            new() { Place = 1, Percent = 50m },
            new() { Place = 2, Percent = 30m },
            new() { Place = 3, Percent = 20m },
        ],
    };

    [Fact]
    public void PrizePool_AndPurses_AreSplitCorrectly()
    {
        var config = MakeConfig();
        var players = new List<TournamentPlayerData>
        {
            new() { PlayerId = 1, PlayerName = "A", CourseHandicap = 0, GrossScores = Enumerable.Repeat(4, 18).ToArray() },
            new() { PlayerId = 2, PlayerName = "B", CourseHandicap = 5, GrossScores = Enumerable.Repeat(5, 18).ToArray() },
            new() { PlayerId = 3, PlayerName = "C", CourseHandicap = 10, GrossScores = Enumerable.Repeat(6, 18).ToArray() },
        };

        var result = _calc.Calculate(config, players);

        // Prize pool = (100 + 3*20) * 0.9 = 144
        Assert.Equal(144m, result.PrizePool);
        Assert.Equal(72m, result.GrossPurse);
        Assert.Equal(72m, result.NetPurse);

        Assert.Equal(43.2m, result.Gross18.Purse);
        Assert.Equal(14.4m, result.GrossFront9.Purse);
        Assert.Equal(14.4m, result.GrossBack9.Purse);
    }

    [Fact]
    public void TieForFirst_SplitsCombinedPlaces()
    {
        var config = MakeConfig();

        // Players A and B tie on gross score, C is third
        var players = new List<TournamentPlayerData>
        {
            new() { PlayerId = 1, PlayerName = "A", CourseHandicap = 0, GrossScores = Enumerable.Repeat(4, 18).ToArray() },
            new() { PlayerId = 2, PlayerName = "B", CourseHandicap = 0, GrossScores = Enumerable.Repeat(4, 18).ToArray() },
            new() { PlayerId = 3, PlayerName = "C", CourseHandicap = 0, GrossScores = Enumerable.Repeat(5, 18).ToArray() },
        };

        var result = _calc.Calculate(config, players);

        // Gross18 purse 43.2
        // Tie for places 1&2 => (50% + 30%) / 2 = 40% each => 17.28 each
        var gross18 = result.Gross18.Entries;
        var a = gross18.First(e => e.PlayerId == 1);
        var b = gross18.First(e => e.PlayerId == 2);

        Assert.Equal(1, a.Place);
        Assert.Equal(1, b.Place);
        Assert.Equal(17.28m, a.Payout);
        Assert.Equal(17.28m, b.Payout);
    }

    [Fact]
    public void NetLeaderboard_UsesHandicapAdjustment()
    {
        var config = MakeConfig();

        var players = new List<TournamentPlayerData>
        {
            new() { PlayerId = 1, PlayerName = "Scratch", CourseHandicap = 0, GrossScores = Enumerable.Repeat(4, 18).ToArray() },
            new() { PlayerId = 2, PlayerName = "Hdcp18", CourseHandicap = 18, GrossScores = Enumerable.Repeat(5, 18).ToArray() },
        };

        var result = _calc.Calculate(config, players);

        var scratch = result.Leaderboard.First(x => x.PlayerId == 1);
        var hdcp = result.Leaderboard.First(x => x.PlayerId == 2);

        Assert.Equal(72, scratch.Gross18);
        Assert.Equal(72, scratch.Net18);
        Assert.Equal(90, hdcp.Gross18);
        Assert.Equal(72, hdcp.Net18); // one stroke per hole
    }

    [Fact]
    public void DefaultPayouts_AreReturned_WhenNotConfigured()
    {
        var defaultsSmall = TournamentCalculator.GetDefaultPlacePayouts(4);
        Assert.Equal(2, defaultsSmall.Count);
        Assert.Equal(60m, defaultsSmall[0].Percent);

        var defaultsMid = TournamentCalculator.GetDefaultPlacePayouts(12);
        Assert.Equal(5, defaultsMid.Count);
        Assert.Equal(40m, defaultsMid[0].Percent);
    }
}
