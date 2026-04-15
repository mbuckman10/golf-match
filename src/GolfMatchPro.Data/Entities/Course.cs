using System.ComponentModel.DataAnnotations;

namespace GolfMatchPro.Data.Entities;

public class Course
{
    public int CourseId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TeeColor { get; set; }

    public int? YearOfInfo { get; set; }

    public decimal CourseRating { get; set; }

    public int SlopeRating { get; set; }

    public ICollection<CourseHole> Holes { get; set; } = [];
}
