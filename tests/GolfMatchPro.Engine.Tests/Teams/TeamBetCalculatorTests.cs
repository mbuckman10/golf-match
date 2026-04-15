using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Investments;
using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Engine.Teams;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Engine.Tests.Teams;

public class TeamBetCalculatorTests
{
    private static readonly int[] StandardRankings = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18];
    private static readonly int[] StandardPars = [4, 4, 3, 5, 4, 4, 3, 5, 4, 4, 4, 3, 5, 4, 4, 3, 5, 4]; // par 72

    #region TeamScoreCalculator

    [Fact]
    public void TeamScore_BestTwoOfFour()
    {
        var player1 = new int[] { 5, 4, 3, 6, 4, 5, 3, 5, 4, 5, 4, 3, 6, 4, 5, 3, 5, 4 };
        var player2 = new int[] { 4, 5, 4, 5, 5, 4, 4, 4, 5, 4, 5, 4, 5, 5, 4, 4, 4, 5 };
        var player3 = new int[] { 6, 3, 5, 4, 3, 6, 5, 6, 3, 6, 3, 5, 4, 3, 6, 5, 6, 3 };
        var player4 = new int[] { 3, 6, 4, 7, 4, 3, 4, 5, 5, 3, 6, 4, 7, 4, 3, 4, 5, 5 };

        var result = TeamScoreCalculator.ComputeTeamHoleScores(
            [player1, player2, player3, player4], 2);

        // Hole 1: best 2 of [5,4,6,3] = 3+4 = 7
        Assert.Equal(7, result[0]);
        // Hole 2: best 2 of [4,5,3,6] = 3+4 = 7
        Assert.Equal(7, result[1]);
    }

    [Fact]
    public void TeamScore_BestTwoOfThree()
    {
        var player1 = new int[] { 5, 4, 3, 6, 4, 5, 3, 5, 4, 5, 4, 3, 6, 4, 5, 3, 5, 4 };
        var player2 = new int[] { 4, 5, 4, 5, 5, 4, 4, 4, 5, 4, 5, 4, 5, 5, 4, 4, 4, 5 };
        var player3 = new int[] { 6, 3, 5, 4, 3, 6, 5, 6, 3, 6, 3, 5, 4, 3, 6, 5, 6, 3 };

        var result = TeamScoreCalculator.ComputeTeamHoleScores(
            [player1, player2, player3], 2);

        // Hole 1: best 2 of [5,4,6] = 4+5 = 9
        Assert.Equal(9, result[0]);
    }

    [Fact]
    public void TeamScore_BestThreeOfFive()
    {
        var scores = Enumerable.Range(1, 5).Select(i =>
            Enumerable.Repeat(i + 2, 18).ToArray()).ToList();
        // Players score 3,4,5,6,7 on every hole

        var result = TeamScoreCalculator.ComputeTeamHoleScores(scores, 3);

        // Best 3 of [3,4,5,6,7] = 3+4+5 = 12
        Assert.All(result, s => Assert.Equal(12, s));
    }

    #endregion

    #region InvestmentCalculator

    [Fact]
    public void Investment_AllOverPar_IsOff()
    {
        // All 4 players score 6 on a par 4 hole → all over par (net), no strokes
        var grossScores = Enumerable.Range(0, 4)
            .Select(_ => BuildScores(6)).ToList();
        var strokes = Enumerable.Range(0, 4)
            .Select(_ => new int[18]).ToList(); // no strokes

        var result = InvestmentCalculator.Evaluate(grossScores, StandardPars, strokes);

        Assert.True(result.IsOff[0]); // Hole 1: par 4, everyone scored 6
        Assert.False(result.IsRedemption[0]);
    }

    [Fact]
    public void Investment_AllAtOrUnderPar_IsRedemption()
    {
        // All 4 players score 4 on a par 4 hole
        var grossScores = Enumerable.Range(0, 4)
            .Select(_ => BuildScores(4)).ToList();
        var strokes = Enumerable.Range(0, 4)
            .Select(_ => new int[18]).ToList();

        var result = InvestmentCalculator.Evaluate(grossScores, StandardPars, strokes);

        Assert.True(result.IsRedemption[0]); // Par or better
        Assert.False(result.IsOff[0]);
    }

    [Fact]
    public void Investment_MixedScores_NeitherOffNorRedemption()
    {
        var grossScores = new List<int[]>
        {
            BuildScores(3), // under par
            BuildScores(5), // over par
            BuildScores(4), // at par
            BuildScores(4), // at par
        };
        var strokes = Enumerable.Range(0, 4)
            .Select(_ => new int[18]).ToList();

        var result = InvestmentCalculator.Evaluate(grossScores, StandardPars, strokes);

        Assert.False(result.IsOff[0]);
        Assert.False(result.IsRedemption[0]);
    }

    [Fact]
    public void Investment_CalculateAmount_OffsAndRedemptions()
    {
        var inv = new InvestmentResult { TotalOffs = 3, TotalRedemptions = 2 };
        decimal amount = InvestmentCalculator.CalculateAmount(inv, 6m, 4m, 2, true, true);
        // Offs: -3 * 6 * 2 = -36, Redemptions: +2 * 4 * 2 = +16, Total: -20
        Assert.Equal(-20m, amount);
    }

    #endregion

    #region TotalStrokesCalculator

    [Fact]
    public void TotalStrokes_WithMaxNetCap()
    {
        // Player 1: gross 90, CH 10 → net 80. Player 2: gross 95, CH 5 → net 90 → capped to 82
        var totals = TotalStrokesCalculator.ComputeTeamTotal(
            [90, 95], [10, 5], 82);

        Assert.Equal(80 + 82, totals); // 162
    }

    [Fact]
    public void TotalStrokes_WithoutCap()
    {
        var totals = TotalStrokesCalculator.ComputeTeamTotal(
            [90, 95], [10, 5], null);

        Assert.Equal(80 + 90, totals); // 170
    }

    [Fact]
    public void TotalStrokes_PairwiseResult()
    {
        // Team A: 160, Team B: 165, $2/stroke → A wins by 5 → $10
        decimal result = TotalStrokesCalculator.ComputePairwiseResult(160, 165, 2m);
        Assert.Equal(10m, result);
    }

    #endregion

    #region TeamBetCalculator (Integration)

    [Fact]
    public void TeamBet_ThreeTeams_MatchPlay_FullCalculation()
    {
        var calc = new TeamBetCalculator(new HandicapCalculator(), new NassauCalculator());

        var config = new TeamBetConfig
        {
            CompetitionType = CompetitionType.MatchPlay,
            HandicapPercentage = 100,
            ScoresCountingPerHole = 2,
            NassauFront = 5,
            NassauBack = 5,
            Nassau18 = 5,
            InvestmentOffEnabled = false,
            RedemptionEnabled = false,
            TotalStrokesBetPerStroke = null,
            ExpenseDeductionPct = 10,
            HoleHandicapRankings = StandardRankings,
            HolePars = StandardPars,
        };

        // Team 1: two players, CH 10 each, scoring par
        // Team 2: two players, CH 10 each, scoring bogey
        // Team 3: two players, CH 10 each, scoring double bogey
        var teams = new List<TeamData>
        {
            MakeTeam(1, "Team A", [10, 10], parDelta: 0),
            MakeTeam(2, "Team B", [10, 10], parDelta: 1),
            MakeTeam(3, "Team C", [10, 10], parDelta: 2),
        };

        var results = calc.Calculate(config, teams);

        Assert.Equal(3, results.TeamResults.Count);
        Assert.Equal(3, results.Matchups.Count); // 3C2 = 3

        // Team A (best scores) should have positive grand total
        var teamA = results.TeamResults.First(t => t.TeamNumber == 1);
        Assert.True(teamA.GrandTotal > 0);

        // Team C (worst scores) should have negative grand total
        var teamC = results.TeamResults.First(t => t.TeamNumber == 3);
        Assert.True(teamC.GrandTotal < 0);

        // Expense deduction only applies to winners
        Assert.True(teamA.GrandTotalAfterExpense < teamA.GrandTotal);
        Assert.Equal(teamC.GrandTotal, teamC.GrandTotalAfterExpense); // No deduction for losers

        // Player results should exist for all players
        Assert.Equal(6, results.PlayerResults.Count);
    }

    [Fact]
    public void TeamBet_ExpenseDeduction_OnlyOnWinnings()
    {
        var calc = new TeamBetCalculator(new HandicapCalculator(), new NassauCalculator());

        var config = new TeamBetConfig
        {
            CompetitionType = CompetitionType.MedalPlay,
            HandicapPercentage = 100,
            ScoresCountingPerHole = 2,
            NassauFront = 10,
            NassauBack = 10,
            Nassau18 = 10,
            ExpenseDeductionPct = 10,
            HoleHandicapRankings = StandardRankings,
            HolePars = StandardPars,
        };

        var teams = new List<TeamData>
        {
            MakeTeam(1, "Winners", [5, 5], parDelta: 0),
            MakeTeam(2, "Losers", [5, 5], parDelta: 2),
        };

        var results = calc.Calculate(config, teams);

        var winners = results.TeamResults.First(t => t.TeamNumber == 1);
        var losers = results.TeamResults.First(t => t.TeamNumber == 2);

        // Winners should have 10% deducted
        Assert.Equal(winners.GrandTotal * 0.9m, winners.GrandTotalAfterExpense);
        // Losers should not have deduction (negative amount stays same)
        Assert.Equal(losers.GrandTotal, losers.GrandTotalAfterExpense);
    }

    #endregion

    #region Helpers

    private static int[] BuildScores(int scorePerHole)
    {
        var scores = new int[18];
        Array.Fill(scores, scorePerHole);
        return scores;
    }

    private static TeamData MakeTeam(int teamNum, string name, int[] courseHandicaps, int parDelta)
    {
        var team = new TeamData { TeamNumber = teamNum, TeamName = name };
        for (int p = 0; p < courseHandicaps.Length; p++)
        {
            var grossScores = new int[18];
            for (int h = 0; h < 18; h++)
                grossScores[h] = StandardPars[h] + parDelta;

            team.Players.Add(new TeamPlayerData
            {
                PlayerId = teamNum * 100 + p,
                PlayerName = $"Player {teamNum}-{p}",
                CourseHandicap = courseHandicaps[p],
                GrossScores = grossScores
            });
        }
        return team;
    }

    #endregion
}
