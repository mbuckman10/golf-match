using GolfMatchPro.Data;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/matches/{matchId}/export")]
public class ExportController : ControllerBase
{
    private readonly GolfMatchDbContext _dbContext;
    private readonly ILogger<ExportController> _logger;

    public ExportController(GolfMatchDbContext dbContext, ILogger<ExportController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("scorecard")]
    public async Task<IActionResult> ExportScorecard(int matchId)
    {
        var lines = await BuildScorecardLines(matchId);
        var bytes = BuildPdf("Scorecard", lines);
        return File(bytes, "application/pdf", $"match-{matchId}-scorecard.pdf");
    }

    [HttpGet("results")]
    public async Task<IActionResult> ExportResults(int matchId)
    {
        var lines = await BuildResultsLines(matchId);
        var bytes = BuildPdf("Bet Results", lines);
        return File(bytes, "application/pdf", $"match-{matchId}-results.pdf");
    }

    [HttpGet("grand-totals")]
    public async Task<IActionResult> ExportGrandTotals(int matchId)
    {
        var lines = await BuildGrandTotalsLines(matchId);
        var bytes = BuildPdf("Grand Totals", lines);
        return File(bytes, "application/pdf", $"match-{matchId}-grand-totals.pdf");
    }

    [HttpGet("full-report")]
    public async Task<IActionResult> ExportFullReport(int matchId)
    {
        var lines = new List<string>();
        lines.AddRange(await BuildScorecardLines(matchId));
        lines.Add(string.Empty);
        lines.AddRange(await BuildResultsLines(matchId));
        lines.Add(string.Empty);
        lines.AddRange(await BuildGrandTotalsLines(matchId));

        var bytes = BuildPdf("Full Match Report", lines);
        return File(bytes, "application/pdf", $"match-{matchId}-full-report.pdf");
    }

    private async Task<List<string>> BuildScorecardLines(int matchId)
    {
        var match = await _dbContext.Matches
            .AsNoTracking()
            .Include(m => m.Course)
            .FirstOrDefaultAsync(m => m.MatchId == matchId);

        var scores = await _dbContext.MatchScores
            .AsNoTracking()
            .Include(ms => ms.Player)
            .Where(ms => ms.MatchId == matchId)
            .OrderBy(ms => ms.Player.FullName)
            .ToListAsync();

        if (match is null)
        {
            return ["Match not found."];
        }

        var lines = new List<string>
        {
            $"Match {match.MatchId} - {match.MatchDate}",
            $"Course: {match.Course.Name}",
            "",
            "Players"
        };

        lines.AddRange(scores.Select(s =>
            $"- {s.Player.Nickname ?? s.Player.FullName}: Gross {s.GrossTotal}, Net {s.NetTotal}, Reportable {s.ReportableScore}"));

        return lines;
    }

    private async Task<List<string>> BuildResultsLines(int matchId)
    {
        var results = await _dbContext.BetResults
            .AsNoTracking()
            .Include(br => br.Player)
            .Include(br => br.BetConfiguration)
            .Where(br => br.BetConfiguration.MatchId == matchId)
            .OrderBy(br => br.BetConfiguration.BetType)
            .ThenBy(br => br.Player.FullName)
            .ToListAsync();

        if (results.Count == 0)
        {
            return ["No bet results found."];
        }

        var lines = new List<string> { "Bet Results" };
        lines.AddRange(results.Select(r =>
            $"- {r.BetConfiguration.BetType} | {r.Player.Nickname ?? r.Player.FullName}: {r.WinLossAmount:C}"));

        return lines;
    }

    private async Task<List<string>> BuildGrandTotalsLines(int matchId)
    {
        var totals = await _dbContext.GrandTotals
            .AsNoTracking()
            .Include(gt => gt.Player)
            .Where(gt => gt.MatchId == matchId)
            .OrderByDescending(gt => gt.TotalWinLoss)
            .ToListAsync();

        if (totals.Count == 0)
        {
            return ["No grand totals found."];
        }

        var lines = new List<string> { "Grand Totals" };
        lines.AddRange(totals.Select(t =>
            $"- {t.Player.Nickname ?? t.Player.FullName}: {t.TotalWinLoss:C}"));

        return lines;
    }

    private static byte[] BuildPdf(string title, List<string> lines)
    {
        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        document.Add(new Paragraph(title).SetFontSize(18));
        document.Add(new Paragraph($"Generated: {DateTime.UtcNow:u}").SetFontSize(10));
        document.Add(new Paragraph(" "));

        foreach (var line in lines)
        {
            document.Add(new Paragraph(line).SetFontSize(11));
        }

        document.Close();
        return stream.ToArray();
    }
}
