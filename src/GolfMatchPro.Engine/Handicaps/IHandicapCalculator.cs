namespace GolfMatchPro.Engine.Handicaps;

public interface IHandicapCalculator
{
    int ComputeCourseHandicap(decimal handicapIndex, int slopeRating);
    int ComputePlayingHandicap(int courseHandicap, decimal percentageUsed, int lowestCourseHandicapInGroup);
    int[] DistributeStrokes(int playingHandicap, int[] holeHandicapRankings);
    int ApplyESC(int grossScore, int par, int courseHandicap);
    int ComputeNetScore(int grossTotal, int courseHandicap);
    int ComputeReportableScore(int[] grossScores, int[] holePars, int courseHandicap);
}
