using GolfMatchPro.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Data;

public class GolfMatchDbContext : DbContext
{
    public GolfMatchDbContext(DbContextOptions<GolfMatchDbContext> options) : base(options) { }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseHole> CourseHoles => Set<CourseHole>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchScore> MatchScores => Set<MatchScore>();
    public DbSet<BetConfiguration> BetConfigurations => Set<BetConfiguration>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamPlayer> TeamPlayers => Set<TeamPlayer>();
    public DbSet<BetResult> BetResults => Set<BetResult>();
    public DbSet<RoundRobinResult> RoundRobinResults => Set<RoundRobinResult>();
    public DbSet<GrandTotal> GrandTotals => Set<GrandTotal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Course
        modelBuilder.Entity<Course>(e =>
        {
            e.HasKey(c => c.CourseId);
            e.Property(c => c.CourseRating).HasPrecision(5, 2);
        });

        // CourseHole
        modelBuilder.Entity<CourseHole>(e =>
        {
            e.HasKey(h => h.CourseHoleId);
            e.HasOne(h => h.Course)
                .WithMany(c => c.Holes)
                .HasForeignKey(h => h.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(h => new { h.CourseId, h.HoleNumber }).IsUnique();
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_CourseHole_HoleNumber", "[HoleNumber] >= 1 AND [HoleNumber] <= 18");
                t.HasCheckConstraint("CK_CourseHole_HandicapRanking", "[HandicapRanking] >= 1 AND [HandicapRanking] <= 18");
                t.HasCheckConstraint("CK_CourseHole_Par", "[Par] >= 3 AND [Par] <= 6");
            });
        });

        // Player
        modelBuilder.Entity<Player>(e =>
        {
            e.HasKey(p => p.PlayerId);
            e.Property(p => p.HandicapIndex).HasPrecision(5, 2);
        });

        // Match
        modelBuilder.Entity<Match>(e =>
        {
            e.HasKey(m => m.MatchId);
            e.Property(m => m.MatchName)
                .IsRequired()
                .HasMaxLength(150);
            e.HasOne(m => m.Course)
                .WithMany()
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.CreatedBy)
                .WithMany()
                .HasForeignKey(m => m.CreatedByPlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(m => m.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        // MatchScore
        modelBuilder.Entity<MatchScore>(e =>
        {
            e.HasKey(s => s.MatchScoreId);
            e.HasOne(s => s.Match)
                .WithMany(m => m.Scores)
                .HasForeignKey(s => s.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Player)
                .WithMany()
                .HasForeignKey(s => s.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(s => new { s.MatchId, s.PlayerId }).IsUnique();
        });

        // BetConfiguration
        modelBuilder.Entity<BetConfiguration>(e =>
        {
            e.HasKey(b => b.BetConfigId);
            e.HasOne(b => b.Match)
                .WithMany(m => m.Bets)
                .HasForeignKey(b => b.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(b => b.BetType).HasConversion<string>().HasMaxLength(30);
            e.Property(b => b.CompetitionType).HasConversion<string>().HasMaxLength(20);
            e.Property(b => b.HandicapPercentage).HasPrecision(5, 2);
            e.Property(b => b.NassauFront).HasPrecision(10, 2);
            e.Property(b => b.NassauBack).HasPrecision(10, 2);
            e.Property(b => b.Nassau18).HasPrecision(10, 2);
            e.Property(b => b.TotalStrokesBetPerStroke).HasPrecision(10, 2);
            e.Property(b => b.InvestmentOffAmount).HasPrecision(10, 2);
            e.Property(b => b.RedemptionAmount).HasPrecision(10, 2);
            e.Property(b => b.DunnAmount).HasPrecision(10, 2);
            e.Property(b => b.PressAmount).HasPrecision(10, 2);
            e.Property(b => b.SkinsBuyIn).HasPrecision(10, 2);
            e.Property(b => b.SkinsPerSkinAmount).HasPrecision(10, 2);
            e.Property(b => b.ExpenseDeductionPct).HasPrecision(5, 2);
        });

        // Team
        modelBuilder.Entity<Team>(e =>
        {
            e.HasKey(t => t.TeamId);
            e.HasOne(t => t.BetConfiguration)
                .WithMany(b => b.Teams)
                .HasForeignKey(t => t.BetConfigId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TeamPlayer
        modelBuilder.Entity<TeamPlayer>(e =>
        {
            e.HasKey(tp => tp.TeamPlayerId);
            e.HasOne(tp => tp.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(tp => tp.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(tp => tp.Player)
                .WithMany()
                .HasForeignKey(tp => tp.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(tp => new { tp.TeamId, tp.PlayerId }).IsUnique();
            e.Property(tp => tp.Position).HasConversion<string>().HasMaxLength(10);
        });

        // BetResult
        modelBuilder.Entity<BetResult>(e =>
        {
            e.HasKey(r => r.BetResultId);
            e.HasOne(r => r.BetConfiguration)
                .WithMany(b => b.Results)
                .HasForeignKey(r => r.BetConfigId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Player)
                .WithMany()
                .HasForeignKey(r => r.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(r => r.WinLossAmount).HasPrecision(10, 2);
            e.Property(r => r.NassauFrontResult).HasPrecision(10, 2);
            e.Property(r => r.NassauBackResult).HasPrecision(10, 2);
            e.Property(r => r.Nassau18Result).HasPrecision(10, 2);
            e.Property(r => r.InvestmentResult).HasPrecision(10, 2);
            e.Property(r => r.TotalStrokesResult).HasPrecision(10, 2);
            e.Property(r => r.SkinsAmount).HasPrecision(10, 2);
            e.Property(r => r.PressResult).HasPrecision(10, 2);
        });

        // RoundRobinResult
        modelBuilder.Entity<RoundRobinResult>(e =>
        {
            e.HasKey(r => r.RoundRobinResultId);
            e.HasOne(r => r.Match)
                .WithMany()
                .HasForeignKey(r => r.MatchId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.BetConfiguration)
                .WithMany()
                .HasForeignKey(r => r.BetConfigId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(r => r.RoundRobinType).HasMaxLength(30);
            e.Property(r => r.MatchupsJson).HasColumnType("nvarchar(max)");
            e.Property(r => r.LeaderboardJson).HasColumnType("nvarchar(max)");
            e.HasIndex(r => new { r.MatchId, r.BetConfigId, r.RoundRobinType });
        });

        // GrandTotal
        modelBuilder.Entity<GrandTotal>(e =>
        {
            e.HasKey(g => g.GrandTotalId);
            e.HasOne(g => g.Match)
                .WithMany()
                .HasForeignKey(g => g.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(g => g.Player)
                .WithMany()
                .HasForeignKey(g => g.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(g => g.TotalWinLoss).HasPrecision(10, 2);
            e.HasIndex(g => new { g.MatchId, g.PlayerId }).IsUnique();
        });
    }
}
