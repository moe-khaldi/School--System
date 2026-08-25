using System.ComponentModel.DataAnnotations;

namespace UniversityCourseEnrollment.Models;

public class Student
{
    public int StudentId { get; set; }

    [Required]
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; } = null!;

    [Required, StringLength(30)]
    public string UniversityNumber { get; set; } = string.Empty;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
