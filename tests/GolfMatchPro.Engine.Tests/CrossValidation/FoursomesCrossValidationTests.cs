using GolfMatchPro.Engine.Handicaps;
using GolfMatchPro.Engine.Investments;
using GolfMatchPro.Engine.Nassau;
using GolfMatchPro.Engine.Teams;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Engine.Tests.CrossValidation;

/// <summary>
/// Cross-validation tests using real data from the original Excel template (original-excel-template.xlsm).
/// Course: Oakridge White, Slope: 124, Rating: 70.5, Date: 2025-04-07
/// 5 teams of 4, Medal Play, 2-best-balls-of-foursome, $5 Nassau, $6 OFF, $3 Redemption, $1/stroke.
/// 
/// KNOWN DISCREPANCIES vs Excel:
/// 1. The Excel uses RAW course handicaps for stroke distribution in Foursomes.
///    The engine's TeamBetCalculator uses playing handicaps relative to the lowest handicap.
///    This affects absolute nassau totals but NOT pairwise binary results (who wins/loses).
/// 2. The Excel's investment OFF is based on the team's best-N balls being over par,
///    NOT all individual players being over par. Also, the Excel only considers players
///    who have scores (ignores missing players), while the engine skips the hole entirely.
/// 3. The expense deduction in the Excel is proportional ($75/man allocated by winnings)
///    rather than a flat 10% of each winner's total.
/// </summary>
public class FoursomesCrossValidationTests
{
    // Oakridge White course data
    private static readonly int[] Pars = [5, 3, 4, 4, 3, 5, 4, 4, 4, 4, 5, 4, 3, 4, 3, 5, 4, 4];
    private static readonly int[] HdcpRankings = [17, 13, 7, 1, 11, 15, 5, 9, 3, 6, 16, 12, 18, 2, 8, 14, 4, 10];

    // Team 1: Jeremy's — Gross scores from Excel Players-Scores sheet
    private static readonly int[] JeremyGross = [5, 4, 4, 4, 3, 5, 5, 4, 4, 3, 5, 4, 3, 3, 4, 6, 5, 4]; // hdcp -1
    private static readonly int[] GoseGross = [5, 3, 7, 6, 3, 6, 4, 4, 5, 5, 6, 6, 3, 4, 3, 6, 6, 4]; // hdcp 9
    private static readonly int[] JDGross = [6, 3, 5, 5, 4, 8, 5, 5, 7, 4, 5, 4, 3, 4, 3, 8, 4, 5]; // hdcp 10

    // Team 2: Sean's
    private static readonly int[] SeanGross = [4, 3, 4, 4, 3, 5, 4, 3, 5, 4, 5, 4, 3, 5, 4, 4, 4, 4]; // hdcp 2
    private static readonly int[] BrettGross = [6, 3, 5, 5, 4, 7, 5, 4, 4, 4, 6, 4, 3, 5, 3, 4, 4, 4]; // hdcp 8
    private static readonly int[] JensenGross = [5, 3, 4, 6, 4, 5, 6, 3, 5, 5, 6, 5, 3, 6, 4, 4, 5, 5]; // hdcp 13
    private static readonly int[] JackGross = [8, 3, 4, 6, 6, 8, 5, 6, 6, 7, 7, 6, 4, 5, 3, 6, 5, 5]; // hdcp 23

    // Team 3: Jedd's
    private static readonly int[] JeddGross = [6, 3, 4, 5, 3, 4, 4, 5, 5, 4, 5, 4, 3, 4, 4, 4, 5, 4]; // hdcp 3
    private static readonly int[] JuddGross = [5, 4, 4, 7, 4, 6, 5, 4, 5, 4, 5, 6, 3, 4, 3, 6, 5, 4]; // hdcp 8
    private static readonly int[] LanceGross = [6, 3, 5, 4, 4, 5, 6, 4, 5, 6, 6, 5, 5, 4, 3, 5, 5, 4]; // hdcp 14

    // Team 4: Brady's
    private static readonly int[] BradyGross = [6, 3, 4, 4, 3, 4, 4, 5, 4, 4, 4, 4, 4, 3, 4, 6, 5, 4]; // hdcp 5
    private static readonly int[] TomGross = [5, 5, 4, 4, 3, 6, 4, 4, 4, 4, 6, 5, 4, 4, 3, 5, 5, 5]; // hdcp 7
    private static readonly int[] BenGross = [5, 4, 5, 5, 4, 7, 5, 5, 4, 5, 5, 6, 3, 5, 4, 6, 4, 5]; // hdcp 15
    private static readonly int[] BakerGross = [7, 3, 5, 5, 6, 5, 5, 5, 7, 5, 6, 4, 3, 6, 5, 5, 6, 5]; // hdcp 19

    // Team 5: Baugh's
    private static readonly int[] TonyGross = [6, 5, 4, 4, 4, 5, 4, 4, 4, 5, 7, 4, 4, 4, 4, 5, 5, 6]; // hdcp 7
    private static readonly int[] ReddGross = [7, 3, 4, 4, 5, 5, 5, 6, 5, 4, 4, 5, 5, 6, 4, 7, 5, 4]; // hdcp 16
    // Baugh(14), Brandon(3) — no gross scores

    private readonly HandicapCalculator _handicapCalc = new();
    private readonly NassauCalculator _nassauCalc = new();

    #region Handicap Tests

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(5, 5)]
    [InlineData(6, 7)]
    [InlineData(7, 8)]
    [InlineData(8, 9)]
    [InlineData(9, 10)]
    [InlineData(12, 13)]
    [InlineData(13, 14)]
    [InlineData(14, 15)]
    [InlineData(15, 16)]
    [InlineData(17.5, 19)]
    [InlineData(21, 23)]
    [InlineData(25.5, 28)]
    public void CourseHandicap_MatchesExcel(double index, int expected)
    {
        Assert.Equal(expected, _handicapCalc.ComputeCourseHandicap((decimal)index, 124));
    }

    [Fact]
    public void StrokeDistribution_NegativeHandicap_GivesBackStrokes()
    {
        // Jeremy: hdcp -1 → gives 1 stroke on easiest hole (rank 18 = hole 13)
        int[] strokes = _handicapCalc.DistributeStrokes(-1, HdcpRankings);

        Assert.Equal(-1, strokes[12]); // hole 13 (rank 18) gets -1
        Assert.Equal(-1, strokes.Sum()); // total strokes = -1
        Assert.Equal(1, strokes.Count(s => s != 0)); // only 1 hole affected
    }

    [Fact]
    public void StrokeDistribution_VerifyNetScores_JeremyMatchesExcel()
    {
        // Excel Foursomes sheet uses RAW course handicap (not playing-relative)
        // Jeremy: course hdcp = -1, gives 1 stroke on hole 13 (rank 18)
        int[] strokes = _handicapCalc.DistributeStrokes(-1, HdcpRankings);
        int[] net = JeremyGross.Zip(strokes, (g, s) => g - s).ToArray();

        // Excel row 37: Jeremy net scores
        int[] excelNet = [5, 4, 4, 4, 3, 5, 5, 4, 4, 3, 5, 4, 4, 3, 4, 6, 5, 4];
        Assert.Equal(excelNet, net);
        Assert.Equal(76, net.Sum());
    }

    [Fact]
    public void StrokeDistribution_VerifyNetScores_GoseMatchesExcel()
    {
        int[] strokes = _handicapCalc.DistributeStrokes(9, HdcpRankings);
        int[] net = GoseGross.Zip(strokes, (g, s) => g - s).ToArray();

        int[] excelNet = [5, 3, 6, 5, 3, 6, 3, 3, 4, 4, 6, 6, 3, 3, 2, 6, 5, 4];
        Assert.Equal(excelNet, net);
        Assert.Equal(77, net.Sum());
    }

    [Fact]
    public void StrokeDistribution_VerifyNetScores_JDMatchesExcel()
    {
        int[] strokes = _handicapCalc.DistributeStrokes(10, HdcpRankings);
        int[] net = JDGross.Zip(strokes, (g, s) => g - s).ToArray();

        int[] excelNet = [6, 3, 4, 4, 4, 8, 4, 4, 6, 3, 5, 4, 3, 3, 2, 8, 3, 4];
        Assert.Equal(excelNet, net);
        Assert.Equal(78, net.Sum());
    }

    #endregion

    #region Team Nassau (Best 2 of 4) Tests — uses raw course handicaps

    [Theory]
    [InlineData(1, 20, 19, 39)]  // Jeremy's team
    [InlineData(2, 61, 65, 126)] // Sean's team
    [InlineData(3, 21, 22, 43)]  // Jedd's team
    [InlineData(4, 62, 63, 125)] // Brady's team
    [InlineData(5, -9, -8, -17)] // Baugh's team
    public void TeamNassauTotals_MatchExcel(int teamNum, int exFront, int exBack, int exTotal)
    {
        var nets = GetTeamPlayerNets(teamNum);
        int[] teamScores = TeamScoreCalculator.ComputeTeamHoleScores(nets, 2);

        Assert.Equal(exFront, teamScores[..9].Sum());
        Assert.Equal(exBack, teamScores[9..].Sum());
        Assert.Equal(exTotal, teamScores.Sum());
    }

    #endregion

    #region Total Strokes Tests

    [Theory]
    [InlineData(1, 203)] // Jeremy 76 + Gose 77 + JD 78 + Ronnie(-28) = 203
    [InlineData(2, 290)] // Sean 70 + Brett 72 + Jensen 71 + Jack 77 = 290
    [InlineData(3, 201)] // Jedd 73 + Judd 76 + Lance 71 + Raff(-19) = 201
    [InlineData(4, 289)] // Brady 70 + Tom 73 + Ben 72 + Baker 74 = 289
    [InlineData(5, 132)] // Baugh(-14) + Tony 77 + Redd 72 + Brandon(-3) = 132
    public void TotalStrokes_MatchExcel(int teamNum, int expected)
    {
        var (grossTotals, hdcps) = GetTeamGrossTotalsAndHdcps(teamNum);
        int total = TotalStrokesCalculator.ComputeTeamTotal(grossTotals, hdcps, 82);
        Assert.Equal(expected, total);
    }

    #endregion

    #region Pairwise Nassau Results Tests

    [Theory]
    // Every team pair — verify who wins front/back/18 in medal play
    [InlineData(1, 2, true, true, true)]    // T1(20,19,39) vs T2(61,65,126) → T1 wins all
    [InlineData(1, 3, true, true, true)]    // T1(20,19,39) vs T3(21,22,43) → T1 wins all
    [InlineData(1, 4, true, true, true)]    // T1(20,19,39) vs T4(62,63,125) → T1 wins all
    [InlineData(1, 5, false, false, false)] // T1(20,19,39) vs T5(-9,-8,-17) → T5 wins all
    [InlineData(2, 3, false, false, false)] // T2(61,65,126) vs T3(21,22,43) → T3 wins all
    [InlineData(2, 4, true, false, false)]  // T2(61,65,126) vs T4(62,63,125) → T2 wins front, T4 wins back+18
    [InlineData(2, 5, false, false, false)] // T2(61,65,126) vs T5(-9,-8,-17) → T5 wins all
    [InlineData(3, 4, true, true, true)]    // T3(21,22,43) vs T4(62,63,125) → T3 wins all
    [InlineData(3, 5, false, false, false)] // T3(21,22,43) vs T5(-9,-8,-17) → T5 wins all
    [InlineData(4, 5, false, false, false)] // T4(62,63,125) vs T5(-9,-8,-17) → T5 wins all
    public void PairwiseNassau_WinLoseMatchesExcel(
        int teamA, int teamB, bool aWinsFront, bool aWinsBack, bool aWins18)
    {
        var scoresA = TeamScoreCalculator.ComputeTeamHoleScores(GetTeamPlayerNets(teamA), 2);
        var scoresB = TeamScoreCalculator.ComputeTeamHoleScores(GetTeamPlayerNets(teamB), 2);
        NassauResult nassau = _nassauCalc.CalculateMedalPlay(scoresA, scoresB);

        Assert.Equal(aWinsFront, nassau.Front9Result > 0);
        Assert.Equal(aWinsBack, nassau.Back9Result > 0);
        Assert.Equal(aWins18, nassau.Overall18Result > 0);
    }

    #endregion

    #region Full Pairwise Dollar Amounts — matches Excel's team-vs-team matrix

    // Each pairwise $ = Nassau($5×3) + Strokes($1/stroke diff) + Investments per matchup
    // Investments per matchup(A,B) = -A.Offs×$6 + A.Red×$3 + B.Offs×$6 - B.Red×$3
    // Team OFFs/Reds: T1(1,1), T2(1,2), T3(0,0), T4(0,0), T5(0,0)
    [Theory]
    [InlineData(1, 2, 99)]   // $15 nassau + $87 strokes + (-$3) inv = $99
    [InlineData(1, 3, 10)]   // $15 + (-$2) + (-$3) = $10
    [InlineData(1, 4, 98)]   // $15 + $86 + (-$3) = $98
    [InlineData(1, 5, -89)]  // -$15 + (-$71) + (-$3) = -$89
    [InlineData(2, 3, -104)] // -$15 + (-$89) + $0 = -$104
    [InlineData(2, 4, -6)]   // -$5 + (-$1) + $0 = -$6
    [InlineData(2, 5, -173)] // -$15 + (-$158) + $0 = -$173
    [InlineData(3, 4, 103)]  // $15 + $88 + $0 = $103
    [InlineData(3, 5, -84)]  // -$15 + (-$69) + $0 = -$84
    [InlineData(4, 5, -172)] // -$15 + (-$157) + $0 = -$172
    public void PairwiseDollarTotal_MatchesExcel(int teamA, int teamB, int expectedDollarsForA)
    {
        // Compute Nassau component
        var scoresA = TeamScoreCalculator.ComputeTeamHoleScores(GetTeamPlayerNets(teamA), 2);
        var scoresB = TeamScoreCalculator.ComputeTeamHoleScores(GetTeamPlayerNets(teamB), 2);
        NassauResult nassau = _nassauCalc.CalculateMedalPlay(scoresA, scoresB);

        decimal nassauDollars =
            (nassau.Front9Result > 0 ? 5 : nassau.Front9Result < 0 ? -5 : 0) +
            (nassau.Back9Result > 0 ? 5 : nassau.Back9Result < 0 ? -5 : 0) +
            (nassau.Overall18Result > 0 ? 5 : nassau.Overall18Result < 0 ? -5 : 0);

        // Compute Total Strokes component
        var (grossA, hdcpA) = GetTeamGrossTotalsAndHdcps(teamA);
        var (grossB, hdcpB) = GetTeamGrossTotalsAndHdcps(teamB);
        int totalA = TotalStrokesCalculator.ComputeTeamTotal(grossA, hdcpA, 82);
        int totalB = TotalStrokesCalculator.ComputeTeamTotal(grossB, hdcpB, 82);
        decimal strokesDollars = TotalStrokesCalculator.ComputePairwiseResult(totalA, totalB, 1m);

        // Compute Investment component per matchup
        // Investment(A vs B) for A = -A.Offs×$6 + A.Red×$3 + B.Offs×$6 - B.Red×$3
        var (offsA, redsA) = GetTeamInvestmentCounts(teamA);
        var (offsB, redsB) = GetTeamInvestmentCounts(teamB);
        decimal investDollars = -offsA * 6 + redsA * 3 + offsB * 6 - redsB * 3;

        decimal total = nassauDollars + strokesDollars + investDollars;
        Assert.Equal(expectedDollarsForA, total);
    }

    #endregion

    #region Per-Team Totals — matches Excel row 24

    [Theory]
    [InlineData(1, 118)]
    [InlineData(2, -382)]
    [InlineData(3, 113)]
    [InlineData(4, -367)]
    [InlineData(5, 518)]
    public void PerTeamTotal_MatchesExcel(int teamNum, int expectedPerMan)
    {
        // Sum of all pairwise results for this team
        int[] allTeams = [1, 2, 3, 4, 5];
        decimal total = 0;

        foreach (int other in allTeams.Where(t => t != teamNum))
        {
            int a = Math.Min(teamNum, other);
            int b = Math.Max(teamNum, other);
            var scoresA = TeamScoreCalculator.ComputeTeamHoleScores(GetTeamPlayerNets(a), 2);
            var scoresB = TeamScoreCalculator.ComputeTeamHoleScores(GetTeamPlayerNets(b), 2);
            NassauResult nassau = _nassauCalc.CalculateMedalPlay(scoresA, scoresB);

            decimal nassauD =
                (nassau.Front9Result > 0 ? 5 : nassau.Front9Result < 0 ? -5 : 0) +
                (nassau.Back9Result > 0 ? 5 : nassau.Back9Result < 0 ? -5 : 0) +
                (nassau.Overall18Result > 0 ? 5 : nassau.Overall18Result < 0 ? -5 : 0);

            var (grossA, hdcpA) = GetTeamGrossTotalsAndHdcps(a);
            var (grossB, hdcpB) = GetTeamGrossTotalsAndHdcps(b);
            int tA = TotalStrokesCalculator.ComputeTeamTotal(grossA, hdcpA, 82);
            int tB = TotalStrokesCalculator.ComputeTeamTotal(grossB, hdcpB, 82);
            decimal strokesD = TotalStrokesCalculator.ComputePairwiseResult(tA, tB, 1m);

            var (offsA, redsA) = GetTeamInvestmentCounts(a);
            var (offsB, redsB) = GetTeamInvestmentCounts(b);
            decimal investD = -offsA * 6 + redsA * 3 + offsB * 6 - redsB * 3;

            decimal pairwise = nassauD + strokesD + investD;
            total += teamNum == a ? pairwise : -pairwise;
        }

        Assert.Equal(expectedPerMan, total);
    }

    #endregion

    #region Investment Tests — Excel-validated

    [Fact]
    public void InvestmentCalculator_Team2_MatchesExcel()
    {
        // Team 2 (Sean's) — all 4 players have scores
        // Excel uses SMALL(playerNets, N) > par for OFF, MAX(playerNets) <= par for Redemption
        var grossList = new List<int[]> { SeanGross, BrettGross, JensenGross, JackGross };
        var strokesList = new List<int[]>();
        foreach (var hdcp in new[] { 2, 8, 13, 23 })
            strokesList.Add(_handicapCalc.DistributeStrokes(hdcp, HdcpRankings));

        var result = InvestmentCalculator.Evaluate(grossList, Pars, strokesList, 2, 6m, 3m);

        // Excel: OFF on H11 (SMALL(nets,2)=6 > 5=par)
        Assert.Equal(1, result.TotalOffs);
        Assert.True(result.IsOff[10]); // Hole 11 (0-indexed)

        // Excel: Redemptions on H13 and H16 (MAX(nets) <= par, after prior OFF)
        Assert.Equal(2, result.TotalRedemptions);
        Assert.True(result.IsRedemption[12]); // Hole 13
        Assert.True(result.IsRedemption[15]); // Hole 16
    }

    [Fact]
    public void InvestmentCalculator_Team1_MatchesExcel()
    {
        // Team 1 has Ronnie (gross=0 for all holes — absent).
        // Absent player's net = 0 - strokes = very negative, naturally handled by SMALL/MAX.
        // Excel: OFF on H16 (SMALL(nets,2)=6 > 5), Redemption on H18 (MAX=4 <= 4)
        var grossList = new List<int[]> { JeremyGross, GoseGross, JDGross, new int[18] };
        var strokesList = new List<int[]>();
        foreach (var hdcp in new[] { -1, 9, 10, 28 })
            strokesList.Add(_handicapCalc.DistributeStrokes(hdcp, HdcpRankings));

        var result = InvestmentCalculator.Evaluate(grossList, Pars, strokesList, 2, 6m, 3m);

        Assert.Equal(1, result.TotalOffs);
        Assert.True(result.IsOff[15]); // Hole 16
        Assert.Equal(1, result.TotalRedemptions);
        Assert.True(result.IsRedemption[17]); // Hole 18
    }

    #endregion

    #region Helpers

    private List<int[]> GetTeamPlayerNets(int teamNum) => teamNum switch
    {
        1 => ComputePlayerNets([(-1, JeremyGross), (9, GoseGross), (10, JDGross), (28, new int[18])]),
        2 => ComputePlayerNets([(2, SeanGross), (8, BrettGross), (13, JensenGross), (23, JackGross)]),
        3 => ComputePlayerNets([(3, JeddGross), (8, JuddGross), (14, LanceGross), (19, new int[18])]),
        4 => ComputePlayerNets([(5, BradyGross), (7, TomGross), (15, BenGross), (19, BakerGross)]),
        5 => ComputePlayerNets([(14, new int[18]), (7, TonyGross), (16, ReddGross), (3, new int[18])]),
        _ => throw new ArgumentException($"Unknown team {teamNum}")
    };

    private static (int[] grossTotals, int[] hdcps) GetTeamGrossTotalsAndHdcps(int teamNum) => teamNum switch
    {
        1 => ([75, 86, 88, 0], [-1, 9, 10, 28]),
        2 => ([72, 80, 84, 100], [2, 8, 13, 23]),
        3 => ([76, 84, 85, 0], [3, 8, 14, 19]),
        4 => ([75, 80, 87, 93], [5, 7, 15, 19]),
        5 => ([0, 84, 88, 0], [14, 7, 16, 3]),
        _ => throw new ArgumentException($"Unknown team {teamNum}")
    };

    // Excel-verified investment counts (using team-best-balls approach)
    private static (int offs, int reds) GetTeamInvestmentCounts(int teamNum) => teamNum switch
    {
        1 => (1, 1), // OFF hole 16, Redemption hole 18
        2 => (1, 2), // OFF hole 11, Redemptions holes 13, 16
        3 => (0, 0),
        4 => (0, 0),
        5 => (0, 0),
        _ => throw new ArgumentException($"Unknown team {teamNum}")
    };

    private List<int[]> ComputePlayerNets((int courseHdcp, int[] gross)[] players)
    {
        var result = new List<int[]>();
        foreach (var (courseHdcp, gross) in players)
        {
            int[] strokes = _handicapCalc.DistributeStrokes(courseHdcp, HdcpRankings);
            var net = new int[18];
            for (int h = 0; h < 18; h++)
            {
                net[h] = gross[h] - strokes[h];
            }
            result.Add(net);
        }
        return result;
    }

    #endregion
}