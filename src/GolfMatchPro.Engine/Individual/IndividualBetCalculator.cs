using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Engine.Individual;

public class IndividualPlayerData
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int CourseHandicap { get; set; }
    public int[] GrossScores { get; set; } = new int[18];
}

public class IndividualBetConfig
{
    public CompetitionType CompetitionType { get; set; }
    public decimal HandicapPercentage { get; set; } = 100;
    public decimal NassauFront { get; set; }
    public decimal NassauBack { get; set; }
    public decimal Nassau18 { get; set; }
    public bool AutoPressEnabled { get; set; }
    public decimal PressAmount { get; set; }
    public int PressDownThreshold { get; set; } = 2;
    public decimal ExpenseDeductionPct { get; set; }
    public int[] HoleHandicapRankings { get; set; } = new int[18];
    public int[] HolePars { get; set; } = new int[18];
}

public class IndividualMatchup
{
    public int PlayerAId { get; set; }
    public int PlayerBId { get; set; }
}

public class IndividualBetResults
{
    public List<IndividualMatchupResult> Matchups { get; set; } = [];
    public List<IndividualPlayerResult> PlayerResults { get; set; } = [];
}

public class IndividualMatchupResult
{
    public int PlayerAId { get; set; }
    public string PlayerAName { get; set; } = string.Empty;
    public int PlayerBId { get; set; }
    public string PlayerBName { get; set; } = string.Empty;
    public NassauResult Nassau { get; set; } = new();
    public decimal NassauFrontDollars { get; set; }
    public decimal NassauBackDollars { get; set; }
    public decimal Nassau18Dollars { get; set; }
    public List<PressResult> Presses { get; set; } = [];
    public decimal TotalPressAmount { get; set; }
    public decimal TotalAmountPlayerA { get; set; }
}

public class PressResult
{
    public int StartHole { get; set; }
    public int EndHole { get; set; }
    public int Result { get; set; }
    public decimal Amount { get; set; }
}

public class IndividualPlayerResult
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public decimal WinLoss { get; set; }
    public decimal WinLossAfterExpense { get; set; }
}

public interface IIndividualBetCalculator
{
    IndividualBetResults Calculate(
        IndividualBetConfig config,
        List<IndividualPlayerData> players,
        List<IndividualMatchup> matchups);
}

public class IndividualBetCalculator : IIndividualBetCalculator
{
    private readonly IHandicapCalculator _handicapCalc;
    private readonly INassauCalculator _nassauCalc;

    public IndividualBetCalculator(IHandicapCalculator handicapCalc, INassauCalculator nassauCalc)
    {
        _handicapCalc = handicapCalc;
        _nassauCalc = nassauCalc;
    }

    public IndividualBetResults Calculate(
        IndividualBetConfig config,
        List<IndividualPlayerData> players,
        List<IndividualMatchup> matchups)
    {
        var results = new IndividualBetResults();
        if (players.Count < 2 || matchups.Count == 0) return results;

        int lowestCH = players.Min(p => p.CourseHandicap);
        decimal pctUsed = config.HandicapPercentage / 100m;

        // Pre-compute net scores for all players
        var playerNetScores = new Dictionary<int, int[]>();
        foreach (var player in players)
        {
            int playingHandicap = _handicapCalc.ComputePlayingHandicap(
                player.CourseHandicap, pctUsed, lowestCH);
            int[] strokes = _handicapCalc.DistributeStrokes(playingHandicap, config.HoleHandicapRankings);

            var netScores = new int[18];
            for (int h = 0; h < 18; h++)
            {
                if (player.GrossScores[h] > 0)
                    netScores[h] = player.GrossScores[h] - strokes[h];
            }
            playerNetScores[player.PlayerId] = netScores;
        }

        // Accumulate per-player totals
        var playerAmounts = players.ToDictionary(p => p.PlayerId, _ => 0m);

        foreach (var matchup in matchups)
        {
            var playerA = players.First(p => p.PlayerId == matchup.PlayerAId);
            var playerB = players.First(p => p.PlayerId == matchup.PlayerBId);

            var netA = playerNetScores[playerA.PlayerId];
            var netB = playerNetScores[playerB.PlayerId];

            NassauResult nassau = config.CompetitionType == CompetitionType.MatchPlay
                ? _nassauCalc.CalculateMatchPlay(netA, netB)
                : _nassauCalc.CalculateMedalPlay(netA, netB);

            decimal frontDollars = nassau.Front9Result > 0 ? config.NassauFront
                : nassau.Front9Result < 0 ? -config.NassauFront : 0;
            decimal backDollars = nassau.Back9Result > 0 ? config.NassauBack
                : nassau.Back9Result < 0 ? -config.NassauBack : 0;
            decimal overallDollars = nassau.Overall18Result > 0 ? config.Nassau18
                : nassau.Overall18Result < 0 ? -config.Nassau18 : 0;

            // Calculate presses
            var presses = new List<PressResult>();
            decimal totalPress = 0;

            if (config.AutoPressEnabled && config.CompetitionType == CompetitionType.MatchPlay)
            {
                presses = CalculateAutoPresses(nassau.HoleByHoleStatus, config.PressDownThreshold, config.PressAmount);
                totalPress = presses.Sum(p => p.Amount);
            }

            decimal totalA = frontDollars + backDollars + overallDollars + totalPress;

            results.Matchups.Add(new IndividualMatchupResult
            {
                PlayerAId = playerA.PlayerId,
                PlayerAName = playerA.PlayerName,
                PlayerBId = playerB.PlayerId,
                PlayerBName = playerB.PlayerName,
                Nassau = nassau,
                NassauFrontDollars = frontDollars,
                NassauBackDollars = backDollars,
                Nassau18Dollars = overallDollars,
                Presses = presses,
                TotalPressAmount = totalPress,
                TotalAmountPlayerA = totalA,
            });

            playerAmounts[playerA.PlayerId] += totalA;
            playerAmounts[playerB.PlayerId] -= totalA;
        }

        // Build player results
        foreach (var player in players)
        {
            decimal wl = playerAmounts[player.PlayerId];
            decimal afterExpense = wl > 0
                ? wl * (1 - config.ExpenseDeductionPct / 100m)
                : wl;

            results.PlayerResults.Add(new IndividualPlayerResult
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                WinLoss = wl,
                WinLossAfterExpense = afterExpense,
            });
        }

        return results;
    }

    public static List<PressResult> CalculateAutoPresses(
        int[] holeByHoleStatus, int downThreshold, decimal pressAmount)
    {
        var presses = new List<PressResult>();
        // Track active presses: each is the hole it started, using the status at that hole as baseline
        var activePresses = new List<(int startHole, int baselineStatus)>();

        for (int h = 0; h < 18; h++)
        {
            int status = holeByHoleStatus[h];

            // Check if player A falls N-down (status <= -threshold) relative to overall
            // A new press triggers when A is N-down and no existing press already covers this trigger
            if (status <= -downThreshold)
            {
                // Only trigger a new press if the deficit just reached a new multiple of threshold
                // or if all existing presses have already been triggered earlier
                bool needNewPress = true;
                foreach (var press in activePresses)
                {
                    int pressStatus = status - press.baselineStatus;
                    if (pressStatus > -downThreshold)
                    {
                        needNewPress = false;
                        break;
                    }
                }
                if (needNewPress && (activePresses.Count == 0 || activePresses.All(p => (status - p.baselineStatus) <= -downThreshold)))
                {
                    activePresses.Add((h, status));
                }
            }

            // Similarly for player B falling N-down (status >= threshold)
            if (status >= downThreshold)
            {
                bool needNewPress = true;
                foreach (var press in activePresses)
                {
                    int pressStatus = status - press.baselineStatus;
                    if (pressStatus < downThreshold)
                    {
                        needNewPress = false;
                        break;
                    }
                }
                if (needNewPress && (activePresses.Count == 0 || activePresses.All(p => (status - p.baselineStatus) >= downThreshold)))
                {
                    activePresses.Add((h, status));
                }
            }
        }

        // Resolve each press: result is the running status from start hole to end (hole 17 or 8)
        foreach (var (startHole, baselineStatus) in activePresses)
        {
            // Determine which nine this press belongs to for end-hole calculation
            int endHole = startHole < 9 ? 8 : 17;
            int pressResult = holeByHoleStatus[endHole] - baselineStatus;

            decimal amount = pressResult > 0 ? pressAmount
                : pressResult < 0 ? -pressAmount : 0;

            presses.Add(new PressResult
            {
                StartHole = startHole + 1, // 1-based
                EndHole = endHole + 1,
                Result = pressResult,
                Amount = amount,
            });
        }

        return presses;
    }
}
