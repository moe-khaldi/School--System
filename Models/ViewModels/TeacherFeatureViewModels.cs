using System.ComponentModel.DataAnnotations;
using UniversityCourseEnrollment.Models;

namespace UniversityCourseEnrollment.Models.ViewModels;

public class TeacherCoursesViewModel
{
    public string TeacherName { get; set; } = string.Empty;
    public List<Course> Courses { get; set; } = [];
}

public class TeacherCourseStudentsViewModel
{
    public Course Course { get; set; } = null!;
}

public class UpdateGradeViewModel
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal? Grade { get; set; }
}
