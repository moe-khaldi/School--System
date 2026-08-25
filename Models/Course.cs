using System.ComponentModel.DataAnnotations;

namespace UniversityCourseEnrollment.Models;

public class Course : IValidatableObject
{
    public int CourseId { get; set; }

    [Required, StringLength(20)]
    public string CourseCode { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string CourseName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, 6)]
    public int CreditHours { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsCancelled { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
            yield return new ValidationResult("End date must be after start date.", [nameof(StartDate), nameof(EndDate)]);
    }
}
