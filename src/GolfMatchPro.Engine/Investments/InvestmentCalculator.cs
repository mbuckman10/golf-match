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
    /// For each hole, determines if a team "went off" (all members over net par)
    /// or earned a redemption (all members at or under net par).
    /// </summary>
    /// <param name="playerGrossScores">Each player's 18 gross scores</param>
    /// <param name="holePars">Par for each of 18 holes</param>
    /// <param name="playerHandicapStrokes">Each player's stroke allocation per hole (from DistributeStrokes)</param>
    public static InvestmentResult Evaluate(
        List<int[]> playerGrossScores,
        int[] holePars,
        List<int[]> playerHandicapStrokes)
    {
        var result = new InvestmentResult();

        for (int hole = 0; hole < 18; hole++)
        {
            bool allPlayed = true;
            bool allOverPar = true;
            bool allAtOrUnderPar = true;

            for (int p = 0; p < playerGrossScores.Count; p++)
            {
                int gross = playerGrossScores[p][hole];
                if (gross == 0)
                {
                    allPlayed = false;
                    break;
                }

                int netPar = holePars[hole] - playerHandicapStrokes[p][hole];
                int netScore = gross - playerHandicapStrokes[p][hole];

                if (netScore <= netPar)
                    allOverPar = false;
                if (netScore > netPar)
                    allAtOrUnderPar = false;
            }

            if (!allPlayed) continue;

            if (allOverPar)
            {
                result.IsOff[hole] = true;
                result.TotalOffs++;
            }
            if (allAtOrUnderPar)
            {
                result.IsRedemption[hole] = true;
                result.TotalRedemptions++;
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
