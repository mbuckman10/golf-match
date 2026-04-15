namespace GolfMatchPro.Engine.BestBall;

/// <summary>
/// Aggregates Best Ball win/loss across multiple BB bet configurations for a match.
/// Each player's total W/L across all BB bets is summed.
/// </summary>
public class BestBallWinLossSummary
{
    public List<BestBallPlayerSummary> PlayerSummaries { get; set; } = [];
}

public class BestBallPlayerSummary
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public decimal TotalWinLoss { get; set; }
    public decimal TotalWinLossAfterExpense { get; set; }
    public int MatchupsPlayed { get; set; }
    public int MatchupsWon { get; set; }
    public int MatchupsLost { get; set; }
    public int MatchupsTied { get; set; }
}

public static class BestBallWinLossAggregator
{
    /// <summary>
    /// Aggregates multiple BestBallResults into a single W-L summary per player.
    /// </summary>
    public static BestBallWinLossSummary Aggregate(List<BestBallResults> allResults)
    {
        var summary = new BestBallWinLossSummary();
        var playerMap = new Dictionary<int, BestBallPlayerSummary>();

        foreach (var result in allResults)
        {
            foreach (var pr in result.PlayerResults)
            {
                if (!playerMap.TryGetValue(pr.PlayerId, out var ps))
                {
                    ps = new BestBallPlayerSummary
                    {
                        PlayerId = pr.PlayerId,
                        PlayerName = pr.PlayerName,
                    };
                    playerMap[pr.PlayerId] = ps;
                }

                ps.TotalWinLoss += pr.WinLoss;
                ps.TotalWinLossAfterExpense += pr.WinLossAfterExpense;
            }

            // Count matchup W/L/T per player from the matchups
            foreach (var matchup in result.Matchups)
            {
                decimal total = matchup.TotalAmountSheetHanger;

                // Sheet hanger players
                if (playerMap.TryGetValue(matchup.SheetHangerTeamNumber, out _))
                {
                    // We track by team number, but need to find the players for that team
                }

                // Track via player results instead - simpler
            }
        }

        // Count matchups from all results
        foreach (var result in allResults)
        {
            // Group matchups by team to attribute W/L/T to players
            var teamPlayers = result.PlayerResults
                .GroupBy(p => p.TeamNumber)
                .ToDictionary(g => g.Key, g => g.Select(p => p.PlayerId).ToList());

            foreach (var matchup in result.Matchups)
            {
                // Sheet hangers
                if (teamPlayers.TryGetValue(matchup.SheetHangerTeamNumber, out var shPlayerIds))
                {
                    foreach (var pid in shPlayerIds)
                    {
                        if (playerMap.TryGetValue(pid, out var ps))
                        {
                            ps.MatchupsPlayed++;
                            if (matchup.TotalAmountSheetHanger > 0) ps.MatchupsWon++;
                            else if (matchup.TotalAmountSheetHanger < 0) ps.MatchupsLost++;
                            else ps.MatchupsTied++;
                        }
                    }
                }

                // Opponents
                if (teamPlayers.TryGetValue(matchup.OpponentTeamNumber, out var oppPlayerIds))
                {
                    foreach (var pid in oppPlayerIds)
                    {
                        if (playerMap.TryGetValue(pid, out var ps))
                        {
                            ps.MatchupsPlayed++;
                            if (matchup.TotalAmountSheetHanger < 0) ps.MatchupsWon++;
                            else if (matchup.TotalAmountSheetHanger > 0) ps.MatchupsLost++;
                            else ps.MatchupsTied++;
                        }
                    }
                }
            }
        }

        summary.PlayerSummaries = playerMap.Values
            .OrderByDescending(p => p.TotalWinLossAfterExpense)
            .ToList();

        return summary;
    }
}
