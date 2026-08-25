using System.ComponentModel.DataAnnotations;
using UniversityCourseEnrollment.Models.Enums;

namespace UniversityCourseEnrollment.Models;

public class AppUser
{
    public int AppUserId { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    // Plain text is intentionally used only for this learning/demo project.
    [Required, StringLength(200)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public Student? Student { get; set; }
    public Teacher? Teacher { get; set; }
}
