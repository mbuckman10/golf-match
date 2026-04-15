using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Engine.BestBall;

public class BestBallPlayerData
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int CourseHandicap { get; set; }
    public int[] GrossScores { get; set; } = new int[18];
}

public class BestBallTeamPair
{
    public int TeamNumber { get; set; }
    public string? TeamName { get; set; }
    public List<BestBallPlayerData> Players { get; set; } = [];
}

public class BestBallConfig
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

public class BestBallResults
{
    public List<BestBallMatchupResult> Matchups { get; set; } = [];
    public List<BestBallPlayerResult> PlayerResults { get; set; } = [];
}

public class BestBallMatchupResult
{
    public int SheetHangerTeamNumber { get; set; }
    public string? SheetHangerTeamName { get; set; }
    public int OpponentTeamNumber { get; set; }
    public string? OpponentTeamName { get; set; }
    public int[] SheetHangerBestBall { get; set; } = new int[18];
    public int[] OpponentBestBall { get; set; } = new int[18];
    public NassauResult Nassau { get; set; } = new();
    public decimal NassauFrontDollars { get; set; }
    public decimal NassauBackDollars { get; set; }
    public decimal Nassau18Dollars { get; set; }
    public List<Individual.PressResult> Presses { get; set; } = [];
    public decimal TotalPressAmount { get; set; }
    public decimal TotalAmountSheetHanger { get; set; }
}

public class BestBallPlayerResult
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int TeamNumber { get; set; }
    public decimal WinLoss { get; set; }
    public decimal WinLossAfterExpense { get; set; }
}

public interface IBestBallCalculator
{
    BestBallResults Calculate(
        BestBallConfig config,
        BestBallTeamPair sheetHangers,
        List<BestBallTeamPair> opponents);
}

public class BestBallCalculator : IBestBallCalculator
{
    private readonly IHandicapCalculator _handicapCalc;
    private readonly INassauCalculator _nassauCalc;

    public BestBallCalculator(IHandicapCalculator handicapCalc, INassauCalculator nassauCalc)
    {
        _handicapCalc = handicapCalc;
        _nassauCalc = nassauCalc;
    }

    public BestBallResults Calculate(
        BestBallConfig config,
        BestBallTeamPair sheetHangers,
        List<BestBallTeamPair> opponents)
    {
        var results = new BestBallResults();
        if (sheetHangers.Players.Count == 0 || opponents.Count == 0) return results;

        // Gather all players to find lowest course handicap
        var allPlayers = new List<BestBallPlayerData>(sheetHangers.Players);
        foreach (var opp in opponents)
            allPlayers.AddRange(opp.Players);

        int lowestCH = allPlayers.Min(p => p.CourseHandicap);
        decimal pctUsed = config.HandicapPercentage / 100m;

        // Compute net scores for all players
        var playerNetScores = new Dictionary<int, int[]>();
        foreach (var player in allPlayers)
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

        // Compute sheet hanger best ball (best 1 of N per hole)
        int[] shBestBall = ComputeBestBall(sheetHangers.Players, playerNetScores);

        // Per-team accumulators
        var teamAmounts = new Dictionary<int, decimal>
        {
            [sheetHangers.TeamNumber] = 0m
        };
        foreach (var opp in opponents)
            teamAmounts[opp.TeamNumber] = 0m;

        // Calculate each matchup
        foreach (var opp in opponents)
        {
            int[] oppBestBall = ComputeBestBall(opp.Players, playerNetScores);

            NassauResult nassau = config.CompetitionType == CompetitionType.MatchPlay
                ? _nassauCalc.CalculateMatchPlay(shBestBall, oppBestBall)
                : _nassauCalc.CalculateMedalPlay(shBestBall, oppBestBall);

            decimal frontDollars = nassau.Front9Result > 0 ? config.NassauFront
                : nassau.Front9Result < 0 ? -config.NassauFront : 0;
            decimal backDollars = nassau.Back9Result > 0 ? config.NassauBack
                : nassau.Back9Result < 0 ? -config.NassauBack : 0;
            decimal overallDollars = nassau.Overall18Result > 0 ? config.Nassau18
                : nassau.Overall18Result < 0 ? -config.Nassau18 : 0;

            var presses = new List<Individual.PressResult>();
            decimal totalPress = 0;

            if (config.AutoPressEnabled && config.CompetitionType == CompetitionType.MatchPlay)
            {
                presses = Individual.IndividualBetCalculator.CalculateAutoPresses(
                    nassau.HoleByHoleStatus, config.PressDownThreshold, config.PressAmount);
                totalPress = presses.Sum(p => p.Amount);
            }

            decimal totalSH = frontDollars + backDollars + overallDollars + totalPress;

            results.Matchups.Add(new BestBallMatchupResult
            {
                SheetHangerTeamNumber = sheetHangers.TeamNumber,
                SheetHangerTeamName = sheetHangers.TeamName,
                OpponentTeamNumber = opp.TeamNumber,
                OpponentTeamName = opp.TeamName,
                SheetHangerBestBall = shBestBall,
                OpponentBestBall = oppBestBall,
                Nassau = nassau,
                NassauFrontDollars = frontDollars,
                NassauBackDollars = backDollars,
                Nassau18Dollars = overallDollars,
                Presses = presses,
                TotalPressAmount = totalPress,
                TotalAmountSheetHanger = totalSH,
            });

            teamAmounts[sheetHangers.TeamNumber] += totalSH;
            teamAmounts[opp.TeamNumber] -= totalSH;
        }

        // Build player results - split evenly within team
        AddPlayerResults(results, sheetHangers, teamAmounts[sheetHangers.TeamNumber], config.ExpenseDeductionPct);
        foreach (var opp in opponents)
        {
            AddPlayerResults(results, opp, teamAmounts[opp.TeamNumber], config.ExpenseDeductionPct);
        }

        return results;
    }

    private static int[] ComputeBestBall(List<BestBallPlayerData> players, Dictionary<int, int[]> netScores)
    {
        var bestBall = new int[18];
        for (int h = 0; h < 18; h++)
        {
            int best = int.MaxValue;
            foreach (var player in players)
            {
                int net = netScores[player.PlayerId][h];
                if (net > 0 && net < best)
                    best = net;
            }
            bestBall[h] = best == int.MaxValue ? 0 : best;
        }
        return bestBall;
    }

    private static void AddPlayerResults(
        BestBallResults results,
        BestBallTeamPair team,
        decimal teamTotal,
        decimal expensePct)
    {
        int count = team.Players.Count;
        if (count == 0) return;

        decimal perPlayer = teamTotal / count;
        decimal afterExpense = teamTotal > 0
            ? (teamTotal * (1 - expensePct / 100m)) / count
            : perPlayer;

        foreach (var player in team.Players)
        {
            results.PlayerResults.Add(new BestBallPlayerResult
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                TeamNumber = team.TeamNumber,
                WinLoss = perPlayer,
                WinLossAfterExpense = afterExpense,
            });
        }
    }
}
