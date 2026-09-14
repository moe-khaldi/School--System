using UniversityCourseEnrollment.Models;

namespace UniversityCourseEnrollment.Models.ViewModels;

public class StudentCoursesViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public List<Course> Courses { get; set; } = [];
}

public class StudentEnrollmentsViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public List<Enrollment> Enrollments { get; set; } = [];
}
