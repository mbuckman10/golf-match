namespace GolfMatchPro.Engine.Teams;

public static class TeamScoreCalculator
{
    /// <summary>
    /// For each hole, takes the best N net scores from the team's players.
    /// Returns an array of 18 team hole scores (sum of best N).
    /// </summary>
    public static int[] ComputeTeamHoleScores(List<int[]> playerNetScores, int scoresCountingPerHole)
    {
        if (playerNetScores.Count == 0)
            throw new ArgumentException("Must provide at least one player's scores.");

        // In real rounds, a team can temporarily have fewer posted/valid players than configured.
        // Clamp counting so results still compute instead of failing the whole bet.
        int effectiveCount = Math.Clamp(scoresCountingPerHole, 1, playerNetScores.Count);

        var teamScores = new int[18];

        for (int hole = 0; hole < 18; hole++)
        {
            var holeScores = new List<int>();
            foreach (var playerScores in playerNetScores)
            {
                holeScores.Add(playerScores[hole]);
            }

            holeScores.Sort();
            teamScores[hole] = holeScores.Take(effectiveCount).Sum();
        }

        return teamScores;
    }
}
