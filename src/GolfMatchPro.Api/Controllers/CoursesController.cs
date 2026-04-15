using GolfMatchPro.Data;
using GolfMatchPro.Data.Entities;
using GolfMatchPro.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfMatchPro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController(GolfMatchDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CourseDto>>> GetAll()
    {
        var courses = await db.Courses
            .Include(c => c.Holes)
            .OrderBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync();

        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetById(int id)
    {
        var course = await db.Courses
            .Include(c => c.Holes.OrderBy(h => h.HoleNumber))
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course is null) return NotFound();
        return Ok(MapToDto(course));
    }

    [HttpPost]
    public async Task<ActionResult<CourseDto>> Create(CreateCourseRequest request)
    {
        var errors = ValidateCourseRequest(request);
        if (errors.Count > 0) return BadRequest(new { errors });

        var course = new Course
        {
            Name = request.Name,
            TeeColor = request.TeeColor,
            YearOfInfo = request.YearOfInfo,
            CourseRating = request.CourseRating,
            SlopeRating = request.SlopeRating,
            Holes = request.Holes.Select(h => new CourseHole
            {
                HoleNumber = h.HoleNumber,
                Par = h.Par,
                HandicapRanking = h.HandicapRanking
            }).ToList()
        };

        db.Courses.Add(course);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = course.CourseId }, MapToDto(course));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CourseDto>> Update(int id, CreateCourseRequest request)
    {
        var course = await db.Courses
            .Include(c => c.Holes)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        if (course is null) return NotFound();

        var errors = ValidateCourseRequest(request);
        if (errors.Count > 0) return BadRequest(new { errors });

        course.Name = request.Name;
        course.TeeColor = request.TeeColor;
        course.YearOfInfo = request.YearOfInfo;
        course.CourseRating = request.CourseRating;
        course.SlopeRating = request.SlopeRating;

        // Replace holes
        db.CourseHoles.RemoveRange(course.Holes);
        course.Holes = request.Holes.Select(h => new CourseHole
        {
            HoleNumber = h.HoleNumber,
            Par = h.Par,
            HandicapRanking = h.HandicapRanking
        }).ToList();

        await db.SaveChangesAsync();
        return Ok(MapToDto(course));
    }

    private static List<string> ValidateCourseRequest(CreateCourseRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Course name is required.");

        if (request.Holes.Count != 18)
            errors.Add("Exactly 18 holes are required.");

        if (request.Holes.Count == 18)
        {
            var holeNumbers = request.Holes.Select(h => h.HoleNumber).OrderBy(n => n).ToList();
            if (!holeNumbers.SequenceEqual(Enumerable.Range(1, 18)))
                errors.Add("Hole numbers must be 1 through 18.");

            var rankings = request.Holes.Select(h => h.HandicapRanking).OrderBy(r => r).ToList();
            if (!rankings.SequenceEqual(Enumerable.Range(1, 18)))
                errors.Add("Handicap rankings must be unique values 1 through 18.");

            if (request.Holes.Any(h => h.Par < 3 || h.Par > 6))
                errors.Add("Par values must be between 3 and 6.");

            var rankingSum = request.Holes.Sum(h => h.HandicapRanking);
            if (rankingSum != 171)
                errors.Add($"Handicap rankings must sum to 171 (got {rankingSum}).");
        }

        if (request.SlopeRating < 55 || request.SlopeRating > 155)
            errors.Add("Slope rating must be between 55 and 155.");

        return errors;
    }

    private static CourseDto MapToDto(Course course) => new()
    {
        CourseId = course.CourseId,
        Name = course.Name,
        TeeColor = course.TeeColor,
        YearOfInfo = course.YearOfInfo,
        CourseRating = course.CourseRating,
        SlopeRating = course.SlopeRating,
        Par = course.Holes.Sum(h => h.Par),
        Holes = course.Holes
            .OrderBy(h => h.HoleNumber)
            .Select(h => new CourseHoleDto
            {
                CourseHoleId = h.CourseHoleId,
                HoleNumber = h.HoleNumber,
                Par = h.Par,
                HandicapRanking = h.HandicapRanking
            }).ToList()
    };
}
