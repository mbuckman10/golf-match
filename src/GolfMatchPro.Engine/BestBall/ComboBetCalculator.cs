namespace GolfMatchPro.Engine.BestBall;

/// <summary>
/// Combo bet types that combine multiple teams into aggregate matchups.
/// Wheel: 2 vs 3, Rope: 2 vs 4, Igg: 3 vs 3, Big Wheel: 3 vs 4, Big Igg: 4 vs 4.
/// Each combo generates all possible sub-team pairings and aggregates the best-ball results.
/// </summary>
public enum ComboBetType
{
    Wheel,      // 2 vs 3
    Rope,       // 2 vs 4
    Igg,        // 3 vs 3
    BigWheel,   // 3 vs 4
    BigIgg      // 4 vs 4
}

public class ComboTeam
{
    public string TeamLabel { get; set; } = string.Empty;
    public List<BestBallPlayerData> Players { get; set; } = [];
}

public class ComboBetConfig
{
    public ComboBetType ComboBetType { get; set; }
    public decimal NassauFront { get; set; }
    public decimal NassauBack { get; set; }
    public decimal Nassau18 { get; set; }
    public decimal ExpenseDeductionPct { get; set; }
}

public class ComboBetResult
{
    public ComboBetType ComboBetType { get; set; }
    public List<ComboPairingResult> Pairings { get; set; } = [];
    public List<ComboPlayerResult> PlayerResults { get; set; } = [];
    public decimal MaxExposureTeamA { get; set; }
    public decimal MaxExposureTeamB { get; set; }
}

public class ComboPairingResult
{
    public string TeamALabel { get; set; } = string.Empty;
    public string TeamBLabel { get; set; } = string.Empty;
    public List<string> TeamAPlayerNames { get; set; } = [];
    public List<string> TeamBPlayerNames { get; set; } = [];
    public decimal NassauFrontDollars { get; set; }
    public decimal NassauBackDollars { get; set; }
    public decimal Nassau18Dollars { get; set; }
    public decimal TotalDollars { get; set; }
}

public class ComboPlayerResult
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string TeamLabel { get; set; } = string.Empty;
    public decimal WinLoss { get; set; }
}

public static class ComboBetCalculator
{
    public static (int sideA, int sideB) GetTeamSizes(ComboBetType type) => type switch
    {
        ComboBetType.Wheel => (2, 3),
        ComboBetType.Rope => (2, 4),
        ComboBetType.Igg => (3, 3),
        ComboBetType.BigWheel => (3, 4),
        ComboBetType.BigIgg => (4, 4),
        _ => throw new ArgumentException($"Unknown combo bet type: {type}")
    };

    /// <summary>
    /// Calculate a combo bet. TeamA and TeamB provide the full player rosters for each side.
    /// All possible 2-man sub-team pairings are generated and best-ball results computed.
    /// </summary>
    public static ComboBetResult Calculate(
        ComboBetConfig config,
        ComboTeam teamA,
        ComboTeam teamB,
        int[] teamABestBall,
        int[] teamBBestBall,
        Func<int[], int[], Nassau.NassauResult> nassauFunc)
    {
        var (sideASize, sideBSize) = GetTeamSizes(config.ComboBetType);

        if (teamA.Players.Count < sideASize)
            throw new ArgumentException($"Team A needs at least {sideASize} players for {config.ComboBetType}.");
        if (teamB.Players.Count < sideBSize)
            throw new ArgumentException($"Team B needs at least {sideBSize} players for {config.ComboBetType}.");

        var result = new ComboBetResult { ComboBetType = config.ComboBetType };

        // Generate all 2-player sub-team combinations from each side
        var subTeamsA = GetCombinations(teamA.Players, 2);
        var subTeamsB = GetCombinations(teamB.Players, 2);

        var playerAmounts = new Dictionary<int, decimal>();
        foreach (var p in teamA.Players) playerAmounts[p.PlayerId] = 0;
        foreach (var p in teamB.Players) playerAmounts[p.PlayerId] = 0;

        decimal totalTeamA = 0;
        decimal totalTeamB = 0;

        // Each 2-man sub-team from A plays each 2-man sub-team from B
        foreach (var subA in subTeamsA)
        {
            foreach (var subB in subTeamsB)
            {
                // For combo bets, we use the overall team best ball (not sub-team best ball)
                // as the spec says the full team's best ball is compared
                var nassau = nassauFunc(teamABestBall, teamBBestBall);

                decimal front = nassau.Front9Result > 0 ? config.NassauFront
                    : nassau.Front9Result < 0 ? -config.NassauFront : 0;
                decimal back = nassau.Back9Result > 0 ? config.NassauBack
                    : nassau.Back9Result < 0 ? -config.NassauBack : 0;
                decimal overall = nassau.Overall18Result > 0 ? config.Nassau18
                    : nassau.Overall18Result < 0 ? -config.Nassau18 : 0;
                decimal total = front + back + overall;

                var labelA = string.Join(" & ", subA.Select(p => p.PlayerName));
                var labelB = string.Join(" & ", subB.Select(p => p.PlayerName));

                result.Pairings.Add(new ComboPairingResult
                {
                    TeamALabel = labelA,
                    TeamBLabel = labelB,
                    TeamAPlayerNames = subA.Select(p => p.PlayerName).ToList(),
                    TeamBPlayerNames = subB.Select(p => p.PlayerName).ToList(),
                    NassauFrontDollars = front,
                    NassauBackDollars = back,
                    Nassau18Dollars = overall,
                    TotalDollars = total,
                });

                // Split the pairing result among the sub-team members
                decimal perPlayerA = total / subA.Count;
                decimal perPlayerB = -total / subB.Count;

                foreach (var p in subA) playerAmounts[p.PlayerId] += perPlayerA;
                foreach (var p in subB) playerAmounts[p.PlayerId] += perPlayerB;

                if (total > 0) totalTeamA += total;
                else totalTeamB += Math.Abs(total);
            }
        }

        result.MaxExposureTeamA = totalTeamB; // max A could lose
        result.MaxExposureTeamB = totalTeamA; // max B could lose

        // Build player results
        foreach (var p in teamA.Players)
        {
            result.PlayerResults.Add(new ComboPlayerResult
            {
                PlayerId = p.PlayerId,
                PlayerName = p.PlayerName,
                TeamLabel = teamA.TeamLabel,
                WinLoss = playerAmounts[p.PlayerId],
            });
        }
        foreach (var p in teamB.Players)
        {
            result.PlayerResults.Add(new ComboPlayerResult
            {
                PlayerId = p.PlayerId,
                PlayerName = p.PlayerName,
                TeamLabel = teamB.TeamLabel,
                WinLoss = playerAmounts[p.PlayerId],
            });
        }

        return result;
    }

    private static List<List<BestBallPlayerData>> GetCombinations(List<BestBallPlayerData> players, int k)
    {
        var results = new List<List<BestBallPlayerData>>();
        GenerateCombinations(players, k, 0, [], results);
        return results;
    }

    private static void GenerateCombinations(
        List<BestBallPlayerData> players,
        int k,
        int start,
        List<BestBallPlayerData> current,
        List<List<BestBallPlayerData>> results)
    {
        if (current.Count == k)
        {
            results.Add(new List<BestBallPlayerData>(current));
            return;
        }

        for (int i = start; i < players.Count; i++)
        {
            current.Add(players[i]);
            GenerateCombinations(players, k, i + 1, current, results);
            current.RemoveAt(current.Count - 1);
        }
    }
}
