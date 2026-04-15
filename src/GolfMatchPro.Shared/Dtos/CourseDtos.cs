namespace GolfMatchPro.Shared.Dtos;

public class CourseDto
{
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TeeColor { get; set; }
    public int? YearOfInfo { get; set; }
    public decimal CourseRating { get; set; }
    public int SlopeRating { get; set; }
    public int Par { get; set; }
    public List<CourseHoleDto> Holes { get; set; } = [];
}

public class CourseHoleDto
{
    public int CourseHoleId { get; set; }
    public int HoleNumber { get; set; }
    public int Par { get; set; }
    public int HandicapRanking { get; set; }
}

public class CreateCourseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TeeColor { get; set; }
    public int? YearOfInfo { get; set; }
    public decimal CourseRating { get; set; }
    public int SlopeRating { get; set; }
    public List<CreateCourseHoleRequest> Holes { get; set; } = [];
}

public class CreateCourseHoleRequest
{
    public int HoleNumber { get; set; }
    public int Par { get; set; }
    public int HandicapRanking { get; set; }
}
