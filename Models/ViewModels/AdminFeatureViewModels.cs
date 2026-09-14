using System.ComponentModel.DataAnnotations;
using UniversityCourseEnrollment.Models.Enums;
using UniversityCourseEnrollment.Services;

namespace UniversityCourseEnrollment.Models.ViewModels;

public class CourseDetailsViewModel
{
    public PaginatedList<Enrollment> Enrollments { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public decimal? AverageGrade { get; set; }
}

public class AssignTeacherViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Choose a valid course.")]
    public int CourseId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a valid teacher.")]
    public int TeacherId { get; set; }
}

public class StudentGpaViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string UniversityNumber { get; set; } = string.Empty;
    public decimal Gpa { get; set; }
}

public class StudentEnrollmentsReportViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<Enrollment> Enrollments { get; set; } = [];
}

public class TeacherCurrentCoursesViewModel
{
    public string TeacherName { get; set; } = string.Empty;
    public List<Course> Courses { get; set; } = [];
}
