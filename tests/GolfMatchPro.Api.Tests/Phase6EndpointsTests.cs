using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GolfMatchPro.Data;
using GolfMatchPro.Data.Entities;
using GolfMatchPro.Shared.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Tests;

public class Phase6EndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Phase6EndpointsTests(WebApplicationFactory<Program> factory)
    {
        var dbName = $"GolfMatchPro_Test_{Guid.NewGuid():N}";
        var connection = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:GolfMatchDb", connection);
        });
    }

    [Fact]
    public async Task RoundRobin_Foursome_Calculate_And_Get_ShouldReturnResults()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GolfMatchDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var seed = await SeedRoundRobinScenario(db);

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            $"/api/matches/{seed.MatchId}/round-robin/foursomes/calculate",
            new { betConfigId = seed.BetConfigId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RoundRobinApiResponse>();
        Assert.NotNull(result);
        Assert.Equal(seed.MatchId, result!.MatchId);
        Assert.Equal(seed.BetConfigId, result.BetConfigId);
        Assert.Single(result.Matchups);

        var fetchResponse = await client.GetAsync($"/api/matches/{seed.MatchId}/round-robin/{result.RoundRobinId}");
        Assert.Equal(HttpStatusCode.OK, fetchResponse.StatusCode);

        var fetched = await fetchResponse.Content.ReadFromJsonAsync<RoundRobinApiResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(result.RoundRobinId, fetched!.RoundRobinId);
        Assert.Single(fetched.Matchups);
    }

    [Fact]
    public async Task GrandTotals_Calculate_And_Leaderboard_ShouldReturnPlayerTotals()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GolfMatchDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var seed = await SeedGrandTotalsScenario(db);

        var client = _factory.CreateClient();
        var calculateResponse = await client.PostAsJsonAsync(
            $"/api/matches/{seed.MatchId}/grand-totals/calculate",
            new
            {
                includeFoursomes = true,
                includeThreesomes = true,
                includeFivesomes = true,
                includeIndividual = true,
                includeBestBall = true,
                includeSkinsGross = true,
                includeSkinsNet = true,
                includeIndoTourney = true,
                includeRoundRobins = true
            });

        Assert.Equal(HttpStatusCode.OK, calculateResponse.StatusCode);

        var totals = await calculateResponse.Content.ReadFromJsonAsync<GrandTotalsApiResponse>();
        Assert.NotNull(totals);
        Assert.Equal(seed.MatchId, totals!.MatchId);
        Assert.Equal(2, totals.PlayerTotals.Count);

        var leaderboardResponse = await client.GetAsync($"/api/matches/{seed.MatchId}/grand-totals/leaderboard");
        Assert.Equal(HttpStatusCode.OK, leaderboardResponse.StatusCode);

        var leaderboard = await leaderboardResponse.Content.ReadFromJsonAsync<List<PlayerGrandTotalApiResponse>>();
        Assert.NotNull(leaderboard);
        Assert.Equal(2, leaderboard!.Count);
    }

    [Fact]
    public async Task Export_FullReport_ShouldReturnPdf()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GolfMatchDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        var seed = await SeedGrandTotalsScenario(db);

        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/matches/{seed.MatchId}/export/full-report");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }

    private static async Task<(int MatchId, int BetConfigId)> SeedRoundRobinScenario(GolfMatchDbContext db)
    {
        var course = new Course { Name = "Test Course", CourseRating = 72, SlopeRating = 113 };
        db.Courses.Add(course);

        var p1 = new Player { FullName = "Player 1", HandicapIndex = 5 };
        var p2 = new Player { FullName = "Player 2", HandicapIndex = 8 };
        var p3 = new Player { FullName = "Player 3", HandicapIndex = 10 };
        var p4 = new Player { FullName = "Player 4", HandicapIndex = 12 };
        db.Players.AddRange(p1, p2, p3, p4);
        await db.SaveChangesAsync();

        var match = new Match
        {
            CourseId = course.CourseId,
            MatchDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            CreatedByPlayerId = p1.PlayerId,
            Status = MatchStatus.InProgress
        };
        db.Matches.Add(match);
        await db.SaveChangesAsync();

        db.MatchScores.AddRange(
            CreateScore(match.MatchId, p1.PlayerId, 4),
            CreateScore(match.MatchId, p2.PlayerId, 5),
            CreateScore(match.MatchId, p3.PlayerId, 6),
            CreateScore(match.MatchId, p4.PlayerId, 7));

        var bet = new BetConfiguration
        {
            MatchId = match.MatchId,
            BetType = BetType.Foursome,
            CompetitionType = CompetitionType.MedalPlay,
            ScoresCountingPerHole = 2,
            NassauFront = 5,
            NassauBack = 5,
            Nassau18 = 5,
            InvestmentOffEnabled = true,
            InvestmentOffAmount = 2,
            RedemptionEnabled = true,
            RedemptionAmount = 1
        };
        db.BetConfigurations.Add(bet);
        await db.SaveChangesAsync();

        var t1 = new Team { BetConfigId = bet.BetConfigId, TeamNumber = 1, TeamName = "A" };
        var t2 = new Team { BetConfigId = bet.BetConfigId, TeamNumber = 2, TeamName = "B" };
        db.Teams.AddRange(t1, t2);
        await db.SaveChangesAsync();

        db.TeamPlayers.AddRange(
            new TeamPlayer { TeamId = t1.TeamId, PlayerId = p1.PlayerId, Position = TeamPosition.Captain },
            new TeamPlayer { TeamId = t1.TeamId, PlayerId = p2.PlayerId, Position = TeamPosition.B },
            new TeamPlayer { TeamId = t2.TeamId, PlayerId = p3.PlayerId, Position = TeamPosition.Captain },
            new TeamPlayer { TeamId = t2.TeamId, PlayerId = p4.PlayerId, Position = TeamPosition.B });

        await db.SaveChangesAsync();
        return (match.MatchId, bet.BetConfigId);
    }

    private static async Task<(int MatchId, int BetConfigId)> SeedGrandTotalsScenario(GolfMatchDbContext db)
    {
        var course = new Course { Name = "GT Course", CourseRating = 72, SlopeRating = 113 };
        db.Courses.Add(course);

        var p1 = new Player { FullName = "GT Player 1", HandicapIndex = 4 };
        var p2 = new Player { FullName = "GT Player 2", HandicapIndex = 9 };
        db.Players.AddRange(p1, p2);
        await db.SaveChangesAsync();

        var match = new Match
        {
            CourseId = course.CourseId,
            MatchDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            CreatedByPlayerId = p1.PlayerId,
            Status = MatchStatus.Completed
        };
        db.Matches.Add(match);
        await db.SaveChangesAsync();

        db.MatchScores.AddRange(
            CreateScore(match.MatchId, p1.PlayerId, 4),
            CreateScore(match.MatchId, p2.PlayerId, 5));

        var bet = new BetConfiguration
        {
            MatchId = match.MatchId,
            BetType = BetType.Foursome,
            CompetitionType = CompetitionType.MedalPlay,
            ScoresCountingPerHole = 2,
            NassauFront = 5,
            NassauBack = 5,
            Nassau18 = 5
        };
        db.BetConfigurations.Add(bet);
        await db.SaveChangesAsync();

        db.BetResults.AddRange(
            new BetResult
            {
                BetConfigId = bet.BetConfigId,
                PlayerId = p1.PlayerId,
                WinLossAmount = 20,
                NassauFrontResult = 5,
                NassauBackResult = 5,
                Nassau18Result = 10,
                InvestmentResult = 0,
                TotalStrokesResult = 0
            },
            new BetResult
            {
                BetConfigId = bet.BetConfigId,
                PlayerId = p2.PlayerId,
                WinLossAmount = -20,
                NassauFrontResult = -5,
                NassauBackResult = -5,
                Nassau18Result = -10,
                InvestmentResult = 0,
                TotalStrokesResult = 0
            });

        await db.SaveChangesAsync();
        return (match.MatchId, bet.BetConfigId);
    }

    private static MatchScore CreateScore(int matchId, int playerId, int score)
    {
        var ms = new MatchScore
        {
            MatchId = matchId,
            PlayerId = playerId,
            CourseHandicap = 10,
            GrossTotal = score * 18,
            NetTotal = score * 18,
            ReportableScore = score * 18,
            IsComplete = true
        };

        for (int h = 1; h <= 18; h++)
        {
            ms.SetHoleScore(h, score);
        }

        return ms;
    }

    private sealed class RoundRobinApiResponse
    {
        public int RoundRobinId { get; set; }
        public int MatchId { get; set; }
        public int BetConfigId { get; set; }
        public List<object> Matchups { get; set; } = [];
    }

    private sealed class GrandTotalsApiResponse
    {
        public int MatchId { get; set; }
        public List<PlayerGrandTotalApiResponse> PlayerTotals { get; set; } = [];
    }

    private sealed class PlayerGrandTotalApiResponse
    {
        public int PlayerId { get; set; }
        public decimal TotalWinLoss { get; set; }
    }
}
