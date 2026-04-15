namespace GolfMatchPro.Engine.Teams;

public static class TotalStrokesCalculator
{
    /// <summary>
    /// Computes team total net strokes, optionally capping each player's net score.
    /// </summary>
    /// <param name="playerGrossTotals">Each player's gross total</param>
    /// <param name="playerCourseHandicaps">Each player's course handicap</param>
    /// <param name="maxNetScore">Optional cap per player's net score</param>
    public static int ComputeTeamTotal(
        int[] playerGrossTotals,
        int[] playerCourseHandicaps,
        int? maxNetScore)
    {
        int total = 0;
        for (int i = 0; i < playerGrossTotals.Length; i++)
        {
            int net = playerGrossTotals[i] - playerCourseHandicaps[i];
            if (maxNetScore.HasValue && net > maxNetScore.Value)
                net = maxNetScore.Value;
            total += net;
        }
        return total;
    }

    /// <summary>
    /// Computes the pairwise total strokes result between two teams.
    /// Returns positive if teamA wins (lower total), negative if teamB wins.
    /// Dollar amount = difference × betPerStroke.
    /// </summary>
    public static decimal ComputePairwiseResult(int teamATotal, int teamBTotal, decimal betPerStroke)
    {
        int diff = teamBTotal - teamATotal; // positive = A is better
        return diff * betPerStroke;
    }
}
