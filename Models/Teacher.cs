using System.ComponentModel.DataAnnotations;

namespace UniversityCourseEnrollment.Models;

public class Teacher
{
    public int TeacherId { get; set; }

    [Required]
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;

    [StringLength(100)]
    public string? Department { get; set; }

    public ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();
}
