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
        if (scoresCountingPerHole > playerNetScores.Count)
            throw new ArgumentException("Scores counting per hole cannot exceed number of players.");

        var teamScores = new int[18];

        for (int hole = 0; hole < 18; hole++)
        {
            var holeScores = new List<int>();
            foreach (var playerScores in playerNetScores)
            {
                holeScores.Add(playerScores[hole]);
            }

            if (holeScores.Count >= scoresCountingPerHole)
            {
                holeScores.Sort();
                teamScores[hole] = holeScores.Take(scoresCountingPerHole).Sum();
            }
            else
            {
                // Not enough scores yet — use what we have
                teamScores[hole] = holeScores.Sum();
            }
        }

        return teamScores;
    }
}
