namespace GolfMatchPro.Data.Entities;

public class MatchScore
{
    public int MatchScoreId { get; set; }

    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int CourseHandicap { get; set; }

    public int Hole1 { get; set; }
    public int Hole2 { get; set; }
    public int Hole3 { get; set; }
    public int Hole4 { get; set; }
    public int Hole5 { get; set; }
    public int Hole6 { get; set; }
    public int Hole7 { get; set; }
    public int Hole8 { get; set; }
    public int Hole9 { get; set; }
    public int Hole10 { get; set; }
    public int Hole11 { get; set; }
    public int Hole12 { get; set; }
    public int Hole13 { get; set; }
    public int Hole14 { get; set; }
    public int Hole15 { get; set; }
    public int Hole16 { get; set; }
    public int Hole17 { get; set; }
    public int Hole18 { get; set; }

    public int GrossTotal { get; set; }
    public int NetTotal { get; set; }
    public int ReportableScore { get; set; }
    public bool IsComplete { get; set; }

    public int[] GetHoleScores() =>
    [
        Hole1, Hole2, Hole3, Hole4, Hole5, Hole6,
        Hole7, Hole8, Hole9, Hole10, Hole11, Hole12,
        Hole13, Hole14, Hole15, Hole16, Hole17, Hole18
    ];

    public void SetHoleScore(int holeNumber, int score)
    {
        switch (holeNumber)
        {
            case 1: Hole1 = score; break;
            case 2: Hole2 = score; break;
            case 3: Hole3 = score; break;
            case 4: Hole4 = score; break;
            case 5: Hole5 = score; break;
            case 6: Hole6 = score; break;
            case 7: Hole7 = score; break;
            case 8: Hole8 = score; break;
            case 9: Hole9 = score; break;
            case 10: Hole10 = score; break;
            case 11: Hole11 = score; break;
            case 12: Hole12 = score; break;
            case 13: Hole13 = score; break;
            case 14: Hole14 = score; break;
            case 15: Hole15 = score; break;
            case 16: Hole16 = score; break;
            case 17: Hole17 = score; break;
            case 18: Hole18 = score; break;
            default: throw new ArgumentOutOfRangeException(nameof(holeNumber), "Hole number must be 1-18.");
        }
    }

    public int GetHoleScore(int holeNumber) => holeNumber switch
    {
        1 => Hole1, 2 => Hole2, 3 => Hole3, 4 => Hole4,
        5 => Hole5, 6 => Hole6, 7 => Hole7, 8 => Hole8,
        9 => Hole9, 10 => Hole10, 11 => Hole11, 12 => Hole12,
        13 => Hole13, 14 => Hole14, 15 => Hole15, 16 => Hole16,
        17 => Hole17, 18 => Hole18,
        _ => throw new ArgumentOutOfRangeException(nameof(holeNumber), "Hole number must be 1-18.")
    };
}
