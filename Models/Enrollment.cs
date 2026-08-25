using System.ComponentModel.DataAnnotations;
using UniversityCourseEnrollment.Models.Enums;

namespace UniversityCourseEnrollment.Models;

public class Enrollment : IValidatableObject
{
    public int EnrollmentId { get; set; }

    [Required]
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    [Required]
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    [Required, DataType(DataType.Date)]
    public DateTime EnrollmentStartDate { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime EnrollmentEndDate { get; set; }

    [Range(0, 100)]
    public decimal? Grade { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EnrollmentEndDate <= EnrollmentStartDate)
            yield return new ValidationResult("Enrollment end date must be after enrollment start date.", [nameof(EnrollmentStartDate), nameof(EnrollmentEndDate)]);
    }
}
