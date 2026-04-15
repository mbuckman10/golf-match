using GolfMatchPro.Engine.Handicaps;

namespace GolfMatchPro.Engine.Tournament;

public class TournamentPlayerData
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int CourseHandicap { get; set; }
    public int[] GrossScores { get; set; } = new int[18];
}

public class PlacePayout
{
    public int Place { get; set; }
    public decimal Percent { get; set; }
}

public class TournamentConfig
{
    public decimal SponsorMoney { get; set; }
    public decimal BuyInPerPlayer { get; set; } = 20m;
    public decimal ExpenseDeductionPct { get; set; }

    public decimal HandicapPercentage { get; set; } = 100m;
    public int[] HoleHandicapRankings { get; set; } = new int[18];

    // Purse split between gross and net (must sum to 100)
    public decimal GrossPursePercent { get; set; } = 50m;
    public decimal NetPursePercent { get; set; } = 50m;

    // Division split within each purse (must sum to 100)
    public decimal EighteenHolePercent { get; set; } = 60m;
    public decimal FrontNinePercent { get; set; } = 20m;
    public decimal BackNinePercent { get; set; } = 20m;

    // Place payouts per division (sum should be <= 100)
    public List<PlacePayout> PlacePayouts { get; set; } = [];
}

public class TournamentDivisionEntry
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Place { get; set; }
    public decimal Payout { get; set; }
}

public class TournamentDivisionResult
{
    public string Name { get; set; } = string.Empty;
    public decimal Purse { get; set; }
    public List<TournamentDivisionEntry> Entries { get; set; } = [];
}

public class TournamentLeaderboardEntry
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Gross18 { get; set; }
    public int Net18 { get; set; }
    public decimal GrossPayout { get; set; }
    public decimal NetPayout { get; set; }
    public decimal TotalPayout { get; set; }
}

public class TournamentResult
{
    public decimal PrizePool { get; set; }
    public decimal GrossPurse { get; set; }
    public decimal NetPurse { get; set; }

    public TournamentDivisionResult Gross18 { get; set; } = new();
    public TournamentDivisionResult GrossFront9 { get; set; } = new();
    public TournamentDivisionResult GrossBack9 { get; set; } = new();

    public TournamentDivisionResult Net18 { get; set; } = new();
    public TournamentDivisionResult NetFront9 { get; set; } = new();
    public TournamentDivisionResult NetBack9 { get; set; } = new();

    public List<TournamentLeaderboardEntry> Leaderboard { get; set; } = [];
}

public interface ITournamentCalculator
{
    TournamentResult Calculate(TournamentConfig config, List<TournamentPlayerData> players);
}

public class TournamentCalculator(IHandicapCalculator handicapCalculator) : ITournamentCalculator
{
    public TournamentResult Calculate(TournamentConfig config, List<TournamentPlayerData> players)
    {
        if (players.Count == 0)
            return new TournamentResult();

        ValidateConfig(config);

        var payoutTable = config.PlacePayouts.Count > 0
            ? config.PlacePayouts
            : GetDefaultPlacePayouts(players.Count);

        decimal prizePool = (config.SponsorMoney + (config.BuyInPerPlayer * players.Count))
            * (1m - (config.ExpenseDeductionPct / 100m));

        decimal grossPurse = prizePool * (config.GrossPursePercent / 100m);
        decimal netPurse = prizePool * (config.NetPursePercent / 100m);

        var grossScores = players.ToDictionary(p => p.PlayerId, p => p.GrossScores.ToArray());
        var netScores = BuildNetScores(config, players);

        var playerById = players.ToDictionary(p => p.PlayerId);

        var result = new TournamentResult
        {
            PrizePool = prizePool,
            GrossPurse = grossPurse,
            NetPurse = netPurse,
            Gross18 = BuildDivision(
                "Gross 18",
                grossPurse * (config.EighteenHolePercent / 100m),
                players,
                p => grossScores[p.PlayerId].Sum(),
                payoutTable),
            GrossFront9 = BuildDivision(
                "Gross Front 9",
                grossPurse * (config.FrontNinePercent / 100m),
                players,
                p => grossScores[p.PlayerId].Take(9).Sum(),
                payoutTable),
            GrossBack9 = BuildDivision(
                "Gross Back 9",
                grossPurse * (config.BackNinePercent / 100m),
                players,
                p => grossScores[p.PlayerId].Skip(9).Take(9).Sum(),
                payoutTable),
            Net18 = BuildDivision(
                "Net 18",
                netPurse * (config.EighteenHolePercent / 100m),
                players,
                p => netScores[p.PlayerId].Sum(),
                payoutTable),
            NetFront9 = BuildDivision(
                "Net Front 9",
                netPurse * (config.FrontNinePercent / 100m),
                players,
                p => netScores[p.PlayerId].Take(9).Sum(),
                payoutTable),
            NetBack9 = BuildDivision(
                "Net Back 9",
                netPurse * (config.BackNinePercent / 100m),
                players,
                p => netScores[p.PlayerId].Skip(9).Take(9).Sum(),
                payoutTable),
        };

        var grossPayouts = SumPayoutsByPlayer(
            result.Gross18,
            result.GrossFront9,
            result.GrossBack9);
        var netPayouts = SumPayoutsByPlayer(
            result.Net18,
            result.NetFront9,
            result.NetBack9);

        result.Leaderboard = players
            .Select(p =>
            {
                grossPayouts.TryGetValue(p.PlayerId, out decimal grossPay);
                netPayouts.TryGetValue(p.PlayerId, out decimal netPay);

                return new TournamentLeaderboardEntry
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    Gross18 = grossScores[p.PlayerId].Sum(),
                    Net18 = netScores[p.PlayerId].Sum(),
                    GrossPayout = grossPay,
                    NetPayout = netPay,
                    TotalPayout = grossPay + netPay,
                };
            })
            .OrderByDescending(x => x.TotalPayout)
            .ThenBy(x => x.Net18)
            .ThenBy(x => x.Gross18)
            .ToList();

        return result;
    }

    public static List<PlacePayout> GetDefaultPlacePayouts(int fieldSize)
    {
        if (fieldSize <= 5)
            return [new() { Place = 1, Percent = 60m }, new() { Place = 2, Percent = 40m }];

        if (fieldSize <= 10)
            return [
                new() { Place = 1, Percent = 50m },
                new() { Place = 2, Percent = 30m },
                new() { Place = 3, Percent = 20m },
            ];

        if (fieldSize <= 16)
            return [
                new() { Place = 1, Percent = 40m },
                new() { Place = 2, Percent = 25m },
                new() { Place = 3, Percent = 15m },
                new() { Place = 4, Percent = 10m },
                new() { Place = 5, Percent = 10m },
            ];

        return [
            new() { Place = 1, Percent = 30m },
            new() { Place = 2, Percent = 20m },
            new() { Place = 3, Percent = 15m },
            new() { Place = 4, Percent = 10m },
            new() { Place = 5, Percent = 8m },
            new() { Place = 6, Percent = 7m },
            new() { Place = 7, Percent = 5m },
            new() { Place = 8, Percent = 5m },
        ];
    }

    private static void ValidateConfig(TournamentConfig config)
    {
        if (config.GrossPursePercent + config.NetPursePercent != 100m)
            throw new ArgumentException("GrossPursePercent and NetPursePercent must sum to 100.");

        if (config.EighteenHolePercent + config.FrontNinePercent + config.BackNinePercent != 100m)
            throw new ArgumentException("Division percentages must sum to 100.");

        if (config.BuyInPerPlayer < 0 || config.SponsorMoney < 0)
            throw new ArgumentException("Money values cannot be negative.");
    }

    private Dictionary<int, int[]> BuildNetScores(TournamentConfig config, List<TournamentPlayerData> players)
    {
        var result = new Dictionary<int, int[]>();

        foreach (var player in players)
        {
            int adjustedHandicap = (int)Math.Round(player.CourseHandicap * (config.HandicapPercentage / 100m));
            int[] strokes = handicapCalculator.DistributeStrokes(adjustedHandicap, config.HoleHandicapRankings);

            var net = new int[18];
            for (int h = 0; h < 18; h++)
            {
                int gross = player.GrossScores[h];
                net[h] = gross > 0 ? gross - strokes[h] : 0;
            }

            result[player.PlayerId] = net;
        }

        return result;
    }

    private static TournamentDivisionResult BuildDivision(
        string name,
        decimal purse,
        List<TournamentPlayerData> players,
        Func<TournamentPlayerData, int> scoreSelector,
        List<PlacePayout> payoutTable)
    {
        var entries = players
            .Select(p => new TournamentDivisionEntry
            {
                PlayerId = p.PlayerId,
                PlayerName = p.PlayerName,
                Score = scoreSelector(p),
                Place = 0,
            })
            .OrderBy(e => e.Score)
            .ThenBy(e => e.PlayerName)
            .ToList();

        int index = 0;
        while (index < entries.Count)
        {
            int score = entries[index].Score;
            int tieStart = index;

            while (index < entries.Count && entries[index].Score == score)
                index++;

            int tieCount = index - tieStart;
            int place = tieStart + 1;

            for (int i = tieStart; i < index; i++)
                entries[i].Place = place;

            decimal tiePercent = 0m;
            for (int p = place; p < place + tieCount; p++)
            {
                var pct = payoutTable.FirstOrDefault(x => x.Place == p);
                tiePercent += pct?.Percent ?? 0m;
            }

            decimal perPlayerPayout = tieCount > 0
                ? purse * (tiePercent / 100m) / tieCount
                : 0m;

            for (int i = tieStart; i < index; i++)
                entries[i].Payout = perPlayerPayout;
        }

        return new TournamentDivisionResult
        {
            Name = name,
            Purse = purse,
            Entries = entries,
        };
    }

    private static Dictionary<int, decimal> SumPayoutsByPlayer(params TournamentDivisionResult[] divisions)
    {
        var totals = new Dictionary<int, decimal>();

        foreach (var division in divisions)
        {
            foreach (var entry in division.Entries)
            {
                totals.TryGetValue(entry.PlayerId, out decimal current);
                totals[entry.PlayerId] = current + entry.Payout;
            }
        }

        return totals;
    }
}
