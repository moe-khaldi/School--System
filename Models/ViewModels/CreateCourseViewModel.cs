using System.ComponentModel.DataAnnotations;

namespace UniversityCourseEnrollment.Models.ViewModels;

public class CreateCourseViewModel : IValidatableObject
{
    [Required, StringLength(20)]
    public string CourseCode { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string CourseName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(1, 6)]
    public int CreditHours { get; set; } = 3;

    [Required, DataType(DataType.Date)]
    public DateTime? StartDate { get; set; } = DateTime.Today;

    [Required, DataType(DataType.Date)]
    public DateTime? EndDate { get; set; } = DateTime.Today.AddMonths(3);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate.HasValue && EndDate <= StartDate)
            yield return new ValidationResult("End date must be after start date.", [nameof(EndDate)]);
    }
}
