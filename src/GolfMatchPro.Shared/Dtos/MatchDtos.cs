using GolfMatchPro.Shared.Enums;

namespace GolfMatchPro.Shared.Dtos;

public class MatchDto
{
    public int MatchId { get; set; }
    public string MatchName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? CourseTeeColor { get; set; }
    public DateOnly MatchDate { get; set; }
    public MatchStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public int CreatedByPlayerId { get; set; }
    public int PlayerCount { get; set; }
}

public class MatchDetailDto
{
    public int MatchId { get; set; }
    public string MatchName { get; set; } = string.Empty;
    public CourseDto Course { get; set; } = null!;
    public DateOnly MatchDate { get; set; }
    public MatchStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public int CreatedByPlayerId { get; set; }
    public List<MatchScoreDto> Scores { get; set; } = [];
}

public class MatchScoreDto
{
    public int MatchScoreId { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string? PlayerNickname { get; set; }
    public int CourseHandicap { get; set; }
    public int[] HoleScores { get; set; } = new int[18];
    public int GrossTotal { get; set; }
    public int NetTotal { get; set; }
    public int ReportableScore { get; set; }
    public bool IsComplete { get; set; }
}

public class CreateMatchRequest
{
    public string MatchName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public DateOnly MatchDate { get; set; }
    public int CreatedByPlayerId { get; set; }
    public List<int> PlayerIds { get; set; } = [];
}

public class UpdateScoreRequest
{
    public int HoleNumber { get; set; }
    public int Score { get; set; }
}

public class BulkUpdateScoreRequest
{
    public int[] HoleScores { get; set; } = new int[18];
}
