using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Investments;
using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Engine.Teams;

public class TeamData
{
    public int TeamNumber { get; set; }
    public string? TeamName { get; set; }
    public List<TeamPlayerData> Players { get; set; } = [];
}

public class TeamPlayerData
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int CourseHandicap { get; set; }
    public int[] GrossScores { get; set; } = new int[18]; // 18 holes
}

public class TeamBetConfig
{
    public CompetitionType CompetitionType { get; set; }
    public decimal HandicapPercentage { get; set; } = 100;
    public int ScoresCountingPerHole { get; set; } = 2;
    public decimal NassauFront { get; set; }
    public decimal NassauBack { get; set; }
    public decimal Nassau18 { get; set; }
    public bool InvestmentOffEnabled { get; set; }
    public decimal InvestmentOffAmount { get; set; }
    public bool RedemptionEnabled { get; set; }
    public decimal RedemptionAmount { get; set; }
    public bool DunnEnabled { get; set; }
    public decimal DunnAmount { get; set; }
    public decimal? TotalStrokesBetPerStroke { get; set; }
    public int? MaxNetScore { get; set; }
    public decimal ExpenseDeductionPct { get; set; }
    public int[] HoleHandicapRankings { get; set; } = new int[18];
    public int[] HolePars { get; set; } = new int[18];
}

public class TeamBetResults
{
    public List<TeamResult> TeamResults { get; set; } = [];
    public List<TeamVsTeamResult> Matchups { get; set; } = [];
    public List<PlayerResult> PlayerResults { get; set; } = [];
}

public class TeamResult
{
    public int TeamNumber { get; set; }
    public string? TeamName { get; set; }
    public int[] TeamHoleScores { get; set; } = new int[18];
    public InvestmentResult Investments { get; set; } = new();
    public decimal InvestmentAmount { get; set; }
    public decimal NassauTotal { get; set; }
    public decimal TotalStrokesTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal GrandTotalAfterExpense { get; set; }
    public int TeamNetTotal { get; set; }
}

public class TeamVsTeamResult
{
    public int TeamANumber { get; set; }
    public int TeamBNumber { get; set; }
    public NassauResult Nassau { get; set; } = new();
    public decimal NassauFrontDollars { get; set; }
    public decimal NassauBackDollars { get; set; }
    public decimal Nassau18Dollars { get; set; }
    public decimal TotalStrokesDollars { get; set; }
}

public class PlayerResult
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int TeamNumber { get; set; }
    public decimal WinLoss { get; set; }
    public decimal WinLossAfterExpense { get; set; }
}

public interface ITeamBetCalculator
{
    TeamBetResults Calculate(TeamBetConfig config, List<TeamData> teams);
}

public class TeamBetCalculator : ITeamBetCalculator
{
    private readonly IHandicapCalculator _handicapCalc;
    private readonly INassauCalculator _nassauCalc;

    public TeamBetCalculator(IHandicapCalculator handicapCalc, INassauCalculator nassauCalc)
    {
        _handicapCalc = handicapCalc;
        _nassauCalc = nassauCalc;
    }

    public TeamBetResults Calculate(TeamBetConfig config, List<TeamData> teams)
    {
        var results = new TeamBetResults();
        if (teams.Count < 2) return results;

        // 1. Compute playing handicaps and net scores for all players
        int lowestCH = teams.SelectMany(t => t.Players).Min(p => p.CourseHandicap);
        decimal pctUsed = config.HandicapPercentage / 100m;

        var teamPlayerNetScores = new Dictionary<int, List<int[]>>(); // teamNumber -> list of player net scores
        var teamPlayerGrossScores = new Dictionary<int, List<int[]>>();
        var teamPlayerHandicapStrokes = new Dictionary<int, List<int[]>>();

        foreach (var team in teams)
        {
            var netScoresList = new List<int[]>();
            var grossScoresList = new List<int[]>();
            var strokesList = new List<int[]>();

            foreach (var player in team.Players)
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

                netScoresList.Add(netScores);
                grossScoresList.Add(player.GrossScores);
                strokesList.Add(strokes);
            }

            teamPlayerNetScores[team.TeamNumber] = netScoresList;
            teamPlayerGrossScores[team.TeamNumber] = grossScoresList;
            teamPlayerHandicapStrokes[team.TeamNumber] = strokesList;
        }

        // 2. Compute team hole scores (best N of M)
        var teamHoleScores = new Dictionary<int, int[]>();
        foreach (var team in teams)
        {
            teamHoleScores[team.TeamNumber] = TeamScoreCalculator.ComputeTeamHoleScores(
                teamPlayerNetScores[team.TeamNumber], config.ScoresCountingPerHole);
        }

        // 3. Compute investments per team
        var teamInvestments = new Dictionary<int, InvestmentResult>();
        foreach (var team in teams)
        {
            teamInvestments[team.TeamNumber] = InvestmentCalculator.Evaluate(
                teamPlayerGrossScores[team.TeamNumber],
                config.HolePars,
                teamPlayerHandicapStrokes[team.TeamNumber],
                config.ScoresCountingPerHole,
                config.InvestmentOffAmount,
                config.RedemptionAmount);
        }

        // 4. Compute total strokes per team
        var teamNetTotals = new Dictionary<int, int>();
        foreach (var team in teams)
        {
            var grossTotals = team.Players.Select(p => p.GrossScores.Where(s => s > 0).Sum()).ToArray();
            var courseHandicaps = team.Players.Select(p => p.CourseHandicap).ToArray();
            teamNetTotals[team.TeamNumber] = TotalStrokesCalculator.ComputeTeamTotal(
                grossTotals, courseHandicaps, config.MaxNetScore);
        }

        // 5. Initialize per-team accumulators
        var teamAmounts = teams.ToDictionary(t => t.TeamNumber, _ => new TeamAmounts());
        int opposingCount = teams.Count - 1;

        // 6. Compute pairwise matchups
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                var teamA = teams[i];
                var teamB = teams[j];

                NassauResult nassau = config.CompetitionType == CompetitionType.MatchPlay
                    ? _nassauCalc.CalculateMatchPlay(teamHoleScores[teamA.TeamNumber], teamHoleScores[teamB.TeamNumber])
                    : _nassauCalc.CalculateMedalPlay(teamHoleScores[teamA.TeamNumber], teamHoleScores[teamB.TeamNumber]);

                // Nassau dollars: positive result means A wins
                decimal frontDollars = nassau.Front9Result > 0 ? config.NassauFront
                    : nassau.Front9Result < 0 ? -config.NassauFront : 0;
                decimal backDollars = nassau.Back9Result > 0 ? config.NassauBack
                    : nassau.Back9Result < 0 ? -config.NassauBack : 0;
                decimal overallDollars = nassau.Overall18Result > 0 ? config.Nassau18
                    : nassau.Overall18Result < 0 ? -config.Nassau18 : 0;

                // Total strokes
                decimal totalStrokesDollars = 0;
                if (config.TotalStrokesBetPerStroke.HasValue)
                {
                    totalStrokesDollars = TotalStrokesCalculator.ComputePairwiseResult(
                        teamNetTotals[teamA.TeamNumber],
                        teamNetTotals[teamB.TeamNumber],
                        config.TotalStrokesBetPerStroke.Value);
                }

                results.Matchups.Add(new TeamVsTeamResult
                {
                    TeamANumber = teamA.TeamNumber,
                    TeamBNumber = teamB.TeamNumber,
                    Nassau = nassau,
                    NassauFrontDollars = frontDollars,
                    NassauBackDollars = backDollars,
                    Nassau18Dollars = overallDollars,
                    TotalStrokesDollars = totalStrokesDollars
                });

                decimal nassauSum = frontDollars + backDollars + overallDollars;
                teamAmounts[teamA.TeamNumber].Nassau += nassauSum;
                teamAmounts[teamB.TeamNumber].Nassau -= nassauSum;

                teamAmounts[teamA.TeamNumber].TotalStrokes += totalStrokesDollars;
                teamAmounts[teamB.TeamNumber].TotalStrokes -= totalStrokesDollars;
            }
        }

        // 7. Compute investment amounts per team
        foreach (var team in teams)
        {
            decimal invAmount = InvestmentCalculator.CalculateAmount(
                teamInvestments[team.TeamNumber],
                config.InvestmentOffAmount,
                config.RedemptionAmount,
                opposingCount,
                config.InvestmentOffEnabled,
                config.RedemptionEnabled);

            if (config.DunnEnabled)
            {
                // Dunn: each redemption also earns dunn amount
                invAmount += teamInvestments[team.TeamNumber].TotalRedemptions * config.DunnAmount * opposingCount;
            }

            teamAmounts[team.TeamNumber].Investment = invAmount;
        }

        // 8. Build team results
        foreach (var team in teams)
        {
            var amt = teamAmounts[team.TeamNumber];
            decimal grand = amt.Nassau + amt.Investment + amt.TotalStrokes;
            decimal afterExpense = grand > 0
                ? grand * (1 - config.ExpenseDeductionPct / 100m)
                : grand; // Only deduct from winnings

            results.TeamResults.Add(new TeamResult
            {
                TeamNumber = team.TeamNumber,
                TeamName = team.TeamName,
                TeamHoleScores = teamHoleScores[team.TeamNumber],
                Investments = teamInvestments[team.TeamNumber],
                InvestmentAmount = amt.Investment,
                NassauTotal = amt.Nassau,
                TotalStrokesTotal = amt.TotalStrokes,
                GrandTotal = grand,
                GrandTotalAfterExpense = afterExpense,
                TeamNetTotal = teamNetTotals[team.TeamNumber]
            });
        }

        // 9. Build per-player results (split evenly among team members)
        foreach (var team in teams)
        {
            var teamResult = results.TeamResults.First(t => t.TeamNumber == team.TeamNumber);
            int playerCount = team.Players.Count;
            if (playerCount == 0) continue;

            decimal perMan = teamResult.GrandTotalAfterExpense / playerCount;

            foreach (var player in team.Players)
            {
                results.PlayerResults.Add(new PlayerResult
                {
                    PlayerId = player.PlayerId,
                    PlayerName = player.PlayerName,
                    TeamNumber = team.TeamNumber,
                    WinLoss = teamResult.GrandTotal / playerCount,
                    WinLossAfterExpense = perMan
                });
            }
        }

        return results;
    }

    private class TeamAmounts
    {
        public decimal Nassau { get; set; }
        public decimal Investment { get; set; }
        public decimal TotalStrokes { get; set; }
    }
}
