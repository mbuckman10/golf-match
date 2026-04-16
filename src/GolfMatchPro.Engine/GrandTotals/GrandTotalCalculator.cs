using GolfMatchPro.Shared.Dtos;
using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Engine.GrandTotals;

/// <summary>
/// Service for aggregating all bet results into grand totals per player.
/// </summary>
public interface IGrandTotalCalculator
{
    /// <summary>
    /// Calculate grand total for a single player across all selected bet types.
    /// </summary>
    PlayerGrandTotalDto Calculate(
        int playerId,
        string playerName,
        Dictionary<string, decimal> betTypeAmounts,
        CalculateGrandTotalsRequest filters);

    /// <summary>
    /// Calculate all grand totals for a match.
    /// </summary>
    GrandTotalsDto CalculateAllPlayers(
        int matchId,
        List<PlayerGrandTotalDto> players,
        CalculateGrandTotalsRequest filters);
}

/// <summary>
/// Data structure to pass bet results by type.
/// </summary>
public class BetTypeAmounts
{
    public decimal Foursomes { get; set; }
    public decimal Threesomes { get; set; }
    public decimal Fivesomes { get; set; }
    public decimal Individual { get; set; }
    public decimal BestBall { get; set; }
    public decimal SkinsGross { get; set; }
    public decimal SkinsNet { get; set; }
    public decimal IndoTourney { get; set; }
    public decimal RoundRobin { get; set; }
}

public class GrandTotalCalculator : IGrandTotalCalculator
{
    public PlayerGrandTotalDto Calculate(
        int playerId,
        string playerName,
        Dictionary<string, decimal> betTypeAmounts,
        CalculateGrandTotalsRequest filters)
    {
        var result = new PlayerGrandTotalDto
        {
            PlayerId = playerId,
            PlayerName = playerName
        };

        decimal grandTotal = 0m;

        // Foursomes
        if (filters.IncludeFoursomes && betTypeAmounts.TryGetValue("Foursomes", out var foursomesAmount))
        {
            result.FoursomesWinLoss = foursomesAmount;
            grandTotal += foursomesAmount;
        }

        // Threesomes
        if (filters.IncludeThreesomes && betTypeAmounts.TryGetValue("Threesomes", out var threesomesAmount))
        {
            result.ThreesomesWinLoss = threesomesAmount;
            grandTotal += threesomesAmount;
        }

        // Fivesomes
        if (filters.IncludeFivesomes && betTypeAmounts.TryGetValue("Fivesomes", out var fivesomesAmount))
        {
            result.FivesomesWinLoss = fivesomesAmount;
            grandTotal += fivesomesAmount;
        }

        // Individual
        if (filters.IncludeIndividual && betTypeAmounts.TryGetValue("Individual", out var individualAmount))
        {
            result.IndividualWinLoss = individualAmount;
            grandTotal += individualAmount;
        }

        // Best Ball
        if (filters.IncludeBestBall && betTypeAmounts.TryGetValue("BestBall", out var bestBallAmount))
        {
            result.BestBallWinLoss = bestBallAmount;
            grandTotal += bestBallAmount;
        }

        // Skins (Gross)
        if (filters.IncludeSkinsGross && betTypeAmounts.TryGetValue("SkinsGross", out var skinsGrossAmount))
        {
            result.SkinsGrossWinLoss = skinsGrossAmount;
            grandTotal += skinsGrossAmount;
        }

        // Skins (Net)
        if (filters.IncludeSkinsNet && betTypeAmounts.TryGetValue("SkinsNet", out var skinsNetAmount))
        {
            result.SkinsNetWinLoss = skinsNetAmount;
            grandTotal += skinsNetAmount;
        }

        // Tournament
        if (filters.IncludeIndoTourney && betTypeAmounts.TryGetValue("Tournament", out var tourneyAmount))
        {
            result.IndoTourneyWinLoss = tourneyAmount;
            grandTotal += tourneyAmount;
        }

        // Round Robin
        if (filters.IncludeRoundRobins && betTypeAmounts.TryGetValue("RoundRobin", out var roundRobinAmount))
        {
            result.RoundRobinWinLoss = roundRobinAmount;
            grandTotal += roundRobinAmount;
        }

        result.TotalWinLoss = grandTotal;
        result.Status = grandTotal > 0 ? "Win" : grandTotal < 0 ? "Loss" : "Break Even";

        return result;
    }

    public GrandTotalsDto CalculateAllPlayers(
        int matchId,
        List<PlayerGrandTotalDto> players,
        CalculateGrandTotalsRequest filters)
    {
        return new GrandTotalsDto
        {
            MatchId = matchId,
            PlayerTotals = players
                .OrderByDescending(p => p.TotalWinLoss)
                .ToList()
        };
    }
}
