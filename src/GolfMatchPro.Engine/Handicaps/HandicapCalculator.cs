namespace GolfMatchPro.Engine.Handicaps;

public class HandicapCalculator : IHandicapCalculator
{
    public int ComputeCourseHandicap(decimal handicapIndex, int slopeRating)
    {
        return (int)Math.Round(handicapIndex * slopeRating / 113m, MidpointRounding.AwayFromZero);
    }

    public int ComputePlayingHandicap(int courseHandicap, decimal percentageUsed, int lowestCourseHandicapInGroup)
    {
        int adjusted = (int)Math.Round(courseHandicap * percentageUsed, MidpointRounding.AwayFromZero);
        int lowestAdjusted = (int)Math.Round(lowestCourseHandicapInGroup * percentageUsed, MidpointRounding.AwayFromZero);
        return adjusted - lowestAdjusted;
    }

    public int[] DistributeStrokes(int playingHandicap, int[] holeHandicapRankings)
    {
        if (holeHandicapRankings.Length != 18)
            throw new ArgumentException("Must provide exactly 18 hole handicap rankings.", nameof(holeHandicapRankings));

        var strokes = new int[18];

        if (playingHandicap > 0)
        {
            // Positive handicap: receive strokes on hardest holes first (lowest ranking number = hardest)
            int fullPasses = playingHandicap / 18;
            int remainder = playingHandicap % 18;

            for (int i = 0; i < 18; i++)
            {
                strokes[i] = fullPasses;
                if (holeHandicapRankings[i] <= remainder)
                    strokes[i]++;
            }
        }
        else if (playingHandicap < 0)
        {
            // Plus handicap (negative): give strokes back on easiest holes first (highest ranking = easiest)
            int absHandicap = Math.Abs(playingHandicap);
            int fullPasses = absHandicap / 18;
            int remainder = absHandicap % 18;

            for (int i = 0; i < 18; i++)
            {
                strokes[i] = -fullPasses;
                // Easiest holes have highest ranking (18 = easiest, 17, 16...)
                // Give back on holes ranked (19 - remainder) through 18
                if (holeHandicapRankings[i] > 18 - remainder)
                    strokes[i]--;
            }
        }

        return strokes;
    }

    public int ApplyESC(int grossScore, int par, int courseHandicap)
    {
        int maxScore = GetESCMaxScore(par, courseHandicap);
        return Math.Min(grossScore, maxScore);
    }

    public int ComputeNetScore(int grossTotal, int courseHandicap)
    {
        return grossTotal - courseHandicap;
    }

    public int ComputeReportableScore(int[] grossScores, int[] holePars, int courseHandicap)
    {
        if (grossScores.Length != 18 || holePars.Length != 18)
            throw new ArgumentException("Must provide exactly 18 scores and pars.");

        int total = 0;
        for (int i = 0; i < 18; i++)
        {
            if (grossScores[i] > 0)
                total += ApplyESC(grossScores[i], holePars[i], courseHandicap);
        }
        return total;
    }

    private static int GetESCMaxScore(int par, int courseHandicap)
    {
        return courseHandicap switch
        {
            <= 4 => par + 2, // Double bogey
            <= 9 => 7,
            <= 14 => 7,
            <= 19 => 7,
            <= 24 => 8,
            <= 29 => 8,
            <= 34 => 9,
            <= 39 => 9,
            _ => 10  // 40+
        };
    }
}
