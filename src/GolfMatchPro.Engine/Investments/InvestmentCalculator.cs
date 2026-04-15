namespace GolfMatchPro.Engine.Investments;

public class InvestmentResult
{
    public bool[] IsOff { get; set; } = new bool[18];
    public bool[] IsRedemption { get; set; } = new bool[18];
    public int TotalOffs { get; set; }
    public int TotalRedemptions { get; set; }
}

public static class InvestmentCalculator
{
    /// <summary>
    /// For each hole, determines if a team "went off" or earned a redemption.
    /// OFF: the N-th smallest player net score exceeds par (team's counting scores all over par).
    /// Redemption: max player net score ≤ par (all players at or under par) AND team has
    /// an outstanding off balance. Absent players (gross=0) get very negative nets from
    /// stroke subtraction, which are naturally handled by the SMALL/MAX sorting.
    /// </summary>
    /// <param name="playerGrossScores">Each player's 18 gross scores (0 = absent)</param>
    /// <param name="holePars">Par for each of 18 holes</param>
    /// <param name="playerHandicapStrokes">Each player's stroke allocation per hole</param>
    /// <param name="scoresCountingPerHole">Best N scores that count (e.g., 2 for foursomes)</param>
    /// <param name="offAmount">Dollar amount per off (used for running balance ratio)</param>
    /// <param name="redemptionAmount">Dollar amount per redemption (used for running balance ratio)</param>
    public static InvestmentResult Evaluate(
        List<int[]> playerGrossScores,
        int[] holePars,
        List<int[]> playerHandicapStrokes,
        int scoresCountingPerHole = 2,
        decimal offAmount = 0,
        decimal redemptionAmount = 0)
    {
        var result = new InvestmentResult();
        int playerCount = playerGrossScores.Count;
        decimal runningBalance = 0;
        decimal redemptionRatio = offAmount > 0 ? redemptionAmount / offAmount : 0;

        for (int hole = 0; hole < 18; hole++)
        {
            // Skip if no player has a score for this hole
            bool anyPlayed = false;
            for (int p = 0; p < playerCount; p++)
            {
                if (playerGrossScores[p][hole] > 0)
                {
                    anyPlayed = true;
                    break;
                }
            }
            if (!anyPlayed) continue;

            // Compute net scores: gross - handicap strokes (including absent players)
            var nets = new int[playerCount];
            for (int p = 0; p < playerCount; p++)
                nets[p] = playerGrossScores[p][hole] - playerHandicapStrokes[p][hole];

            Array.Sort(nets); // ascending
            int par = holePars[hole];

            // OFF: N-th smallest net > par (team's counting scores are all over par)
            int n = Math.Min(scoresCountingPerHole, nets.Length);
            if (nets[n - 1] > par)
            {
                result.IsOff[hole] = true;
                result.TotalOffs++;
                runningBalance++;
            }

            // Redemption: MAX net ≤ par (all players at or under par)
            // When offAmount > 0, requires prior off (running balance > 0)
            bool redemptionEligible = offAmount > 0 ? runningBalance > 0 : true;
            if (nets[^1] <= par && redemptionEligible)
            {
                result.IsRedemption[hole] = true;
                result.TotalRedemptions++;
                if (offAmount > 0)
                    runningBalance -= redemptionRatio;
            }
        }

        return result;
    }

    /// <summary>
    /// Calculate the dollar amount for a team's investments against opposing teams.
    /// </summary>
    public static decimal CalculateAmount(
        InvestmentResult result,
        decimal offAmount,
        decimal redemptionAmount,
        int opposingTeamCount,
        bool offEnabled,
        bool redemptionEnabled)
    {
        decimal total = 0;
        if (offEnabled)
            total -= result.TotalOffs * offAmount * opposingTeamCount;
        if (redemptionEnabled)
            total += result.TotalRedemptions * redemptionAmount * opposingTeamCount;
        return total;
    }
}
