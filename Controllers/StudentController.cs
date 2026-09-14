using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.ViewModels;
using UniversityCourseEnrollment.Services;

namespace UniversityCourseEnrollment.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EnrollmentService _enrollmentService;

    public StudentController(ApplicationDbContext context, EnrollmentService enrollmentService)
    {
        _context = context;
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public Task<IActionResult> Index() => Dashboard();

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var student = await _context.Students
            .Include(x => x.Enrollments)
            .FirstOrDefaultAsync(x => x.AppUserId == userId);

        var model = new StudentDashboardViewModel
        {
            StudentName = User.Identity?.Name ?? "Student",
            EnrollmentCount = student?.Enrollments.Count ?? 0
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> OpenCourses()
    {
        var student = await GetCurrentStudent();
        if (student is null) return NotFound("Student profile was not found.");

        return View(new StudentCoursesViewModel
        {
            StudentId = student.StudentId,
            StudentName = User.Identity?.Name ?? "Student",
            Courses = await _enrollmentService.GetOpenCourses()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id)
    {
        var student = await GetCurrentStudent();
        if (student is null) return NotFound("Student profile was not found.");

        var result = await _enrollmentService.EnrollStudentInCourse(student.StudentId, id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(OpenCourses));
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        var student = await GetCurrentStudent();
        if (student is null) return NotFound("Student profile was not found.");

        return View(new StudentEnrollmentsViewModel
        {
            StudentName = User.Identity?.Name ?? "Student",
            Enrollments = await _enrollmentService.GetStudentEnrollments(student.StudentId)
        });
    }

    private async Task<Student?> GetCurrentStudent()
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdText, out var userId)
            ? await _context.Students.FirstOrDefaultAsync(s => s.AppUserId == userId)
            : null;
    }
}

public class StudentDashboardViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public int EnrollmentCount { get; set; }
}
