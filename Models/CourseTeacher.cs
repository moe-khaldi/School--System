using System.ComponentModel.DataAnnotations;

namespace UniversityCourseEnrollment.Models;

public class CourseTeacher
{
    public int CourseTeacherId { get; set; }

    [Required]
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    [Required]
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
}
