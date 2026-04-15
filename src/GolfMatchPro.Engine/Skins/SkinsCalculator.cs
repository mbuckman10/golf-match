using GolfMatchPro.Engine.Handicaps;

namespace GolfMatchPro.Engine.Skins;

public class SkinsPlayerData
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int CourseHandicap { get; set; }
    public int[] GrossScores { get; set; } = new int[18];
}

public class SkinsConfig
{
    public bool UseNetScores { get; set; }
    public decimal HandicapPercentage { get; set; } = 100;
    public int[] HoleHandicapRankings { get; set; } = new int[18];
    public decimal? BuyInPerPlayer { get; set; }
    public decimal? AmountPerSkin { get; set; }
}

public class SkinsHoleResult
{
    public int HoleNumber { get; set; }
    public int CarryIn { get; set; }
    public int CarryOut { get; set; }
    public int? WinnerPlayerId { get; set; }
    public string? WinnerPlayerName { get; set; }
    public int SkinsAwarded { get; set; }
    public int WinningScore { get; set; }
    public List<int> TiedPlayerIds { get; set; } = [];
}

public class SkinsPlayerResult
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int SkinsWon { get; set; }
    public decimal GrossWinnings { get; set; }
    public decimal NetWinnings { get; set; }
}

public class SkinsResult
{
    public List<SkinsHoleResult> HoleResults { get; set; } = [];
    public List<SkinsPlayerResult> PlayerResults { get; set; } = [];
    public int TotalSkinsAwarded { get; set; }
    public int UnresolvedCarrySkins { get; set; }
    public decimal TotalPot { get; set; }
    public decimal AmountPerAwardedSkin { get; set; }
}

public interface ISkinsCalculator
{
    SkinsResult Calculate(SkinsConfig config, List<SkinsPlayerData> players);
}

public class SkinsCalculator(IHandicapCalculator handicapCalculator) : ISkinsCalculator
{
    public SkinsResult Calculate(SkinsConfig config, List<SkinsPlayerData> players)
    {
        if (players.Count == 0)
            return new SkinsResult();

        if (config.AmountPerSkin.HasValue && config.BuyInPerPlayer.HasValue)
            throw new ArgumentException("Use either AmountPerSkin or BuyInPerPlayer, not both.");

        var scoreCards = BuildScores(config, players);
        var skinsByPlayer = players.ToDictionary(p => p.PlayerId, _ => 0);
        var holeResults = new List<SkinsHoleResult>(18);

        int carry = 0;
        int awarded = 0;

        for (int h = 0; h < 18; h++)
        {
            int carryIn = carry;
            carry += 1;

            var holeScores = new List<(int playerId, string playerName, int score)>();
            foreach (var player in players)
            {
                int score = scoreCards[player.PlayerId][h];
                if (score > 0)
                    holeScores.Add((player.PlayerId, player.PlayerName, score));
            }

            if (holeScores.Count == 0)
            {
                holeResults.Add(new SkinsHoleResult
                {
                    HoleNumber = h + 1,
                    CarryIn = carryIn,
                    CarryOut = carry,
                    SkinsAwarded = 0,
                    WinningScore = 0,
                });
                continue;
            }

            int low = holeScores.Min(x => x.score);
            var tiedLow = holeScores.Where(x => x.score == low).ToList();

            if (tiedLow.Count == 1)
            {
                var winner = tiedLow[0];
                skinsByPlayer[winner.playerId] += carry;
                awarded += carry;

                holeResults.Add(new SkinsHoleResult
                {
                    HoleNumber = h + 1,
                    CarryIn = carryIn,
                    CarryOut = 0,
                    WinnerPlayerId = winner.playerId,
                    WinnerPlayerName = winner.playerName,
                    SkinsAwarded = carry,
                    WinningScore = low,
                });

                carry = 0;
            }
            else
            {
                holeResults.Add(new SkinsHoleResult
                {
                    HoleNumber = h + 1,
                    CarryIn = carryIn,
                    CarryOut = carry,
                    SkinsAwarded = 0,
                    WinningScore = low,
                    TiedPlayerIds = tiedLow.Select(x => x.playerId).ToList(),
                });
            }
        }

        decimal totalPot = config.BuyInPerPlayer.HasValue
            ? config.BuyInPerPlayer.Value * players.Count
            : (config.AmountPerSkin ?? 0m) * awarded;

        decimal perAwardedSkin = awarded > 0
            ? (config.BuyInPerPlayer.HasValue ? totalPot / awarded : config.AmountPerSkin ?? 0m)
            : 0m;

        var playerResults = players
            .Select(p =>
            {
                int skinsWon = skinsByPlayer[p.PlayerId];
                decimal grossWinnings = skinsWon * perAwardedSkin;
                decimal netWinnings = config.BuyInPerPlayer.HasValue
                    ? grossWinnings - config.BuyInPerPlayer.Value
                    : grossWinnings;

                return new SkinsPlayerResult
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    SkinsWon = skinsWon,
                    GrossWinnings = grossWinnings,
                    NetWinnings = netWinnings,
                };
            })
            .OrderByDescending(p => p.SkinsWon)
            .ThenBy(p => p.PlayerName)
            .ToList();

        return new SkinsResult
        {
            HoleResults = holeResults,
            PlayerResults = playerResults,
            TotalSkinsAwarded = awarded,
            UnresolvedCarrySkins = carry,
            TotalPot = totalPot,
            AmountPerAwardedSkin = perAwardedSkin,
        };
    }

    private Dictionary<int, int[]> BuildScores(SkinsConfig config, List<SkinsPlayerData> players)
    {
        var result = new Dictionary<int, int[]>();

        foreach (var player in players)
        {
            if (!config.UseNetScores)
            {
                result[player.PlayerId] = player.GrossScores.ToArray();
                continue;
            }

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
}
