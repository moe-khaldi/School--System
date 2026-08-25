using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models.Enums;

namespace UniversityCourseEnrollment.Services;

public class GpaService
{
    private readonly ApplicationDbContext _context;

    public GpaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public decimal ConvertGradeToPoints(decimal grade) => grade switch
    {
        >= 90 => 4.0m,
        >= 85 => 3.7m,
        >= 80 => 3.3m,
        >= 75 => 3.0m,
        >= 70 => 2.7m,
        >= 65 => 2.3m,
        >= 60 => 2.0m,
        _ => 0.0m
    };

    public async Task<decimal> CalculateStudentGpa(int studentId)
    {
        var records = await _context.Enrollments
            .Where(e => e.StudentId == studentId &&
                        e.Status == EnrollmentStatus.Completed &&
                        e.Grade.HasValue)
            .Select(e => new { Grade = e.Grade!.Value, e.Course.CreditHours })
            .ToListAsync();

        var totalHours = records.Sum(x => x.CreditHours);
        if (totalHours == 0) return 0m;

        return Math.Round(records.Sum(x => ConvertGradeToPoints(x.Grade) * x.CreditHours) / totalHours, 2);
    }

    public async Task<List<StudentGpaResult>> GetTop10StudentsByGpa()
    {
        var students = await _context.Students
            .Include(s => s.AppUser)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .ToListAsync();

        return students
            .Select(student =>
            {
                var graded = student.Enrollments
                    .Where(e => e.Status == EnrollmentStatus.Completed && e.Grade.HasValue)
                    .ToList();
                var totalHours = graded.Sum(e => e.Course.CreditHours);
                var gpa = totalHours == 0
                    ? 0m
                    : Math.Round(graded.Sum(e => ConvertGradeToPoints(e.Grade!.Value) * e.Course.CreditHours) / totalHours, 2);
                var highest = graded.OrderByDescending(e => e.Grade).FirstOrDefault();

                return new StudentGpaResult
                {
                    StudentId = student.StudentId,
                    StudentName = student.AppUser.FullName,
                    UniversityNumber = student.UniversityNumber,
                    Gpa = gpa,
                    HighestGrade = highest?.Grade,
                    HighestGradeCourse = highest?.Course.CourseName
                };
            })
            .OrderByDescending(x => x.Gpa)
            .ThenBy(x => x.StudentName)
            .Take(10)
            .ToList();
    }
}

public class StudentGpaResult
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UniversityNumber { get; set; } = string.Empty;
    public decimal Gpa { get; set; }
    public decimal? HighestGrade { get; set; }
    public string? HighestGradeCourse { get; set; }
}
