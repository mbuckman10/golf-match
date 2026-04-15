namespace GolfMatchPro.Data.Entities;

public class CourseHole
{
    public int CourseHoleId { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int HoleNumber { get; set; }

    public int Par { get; set; }

    public int HandicapRanking { get; set; }
}
