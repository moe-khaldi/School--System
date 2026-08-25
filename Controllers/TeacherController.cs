using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.ViewModels;
using UniversityCourseEnrollment.Services;

namespace UniversityCourseEnrollment.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly TeacherService _teacherService;

    public TeacherController(ApplicationDbContext context, TeacherService teacherService)
    {
        _context = context;
        _teacherService = teacherService;
    }

    [HttpGet]
    public Task<IActionResult> Index() => Dashboard();

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var teacher = await _teacherService.GetTeacherByUserId(userId);

        var model = new TeacherDashboardViewModel
        {
            TeacherName = User.Identity?.Name ?? "Teacher",
            CourseCount = teacher is null ? 0 : await _teacherService.GetTeacherCourseCount(teacher.TeacherId)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        var teacher = await GetCurrentTeacher();
        if (teacher is null) return NotFound("Teacher profile was not found.");

        return View(new TeacherCoursesViewModel
        {
            TeacherName = User.Identity?.Name ?? "Teacher",
            Courses = await _teacherService.GetTeacherCourses(teacher.TeacherId)
        });
    }

    [HttpGet]
    public async Task<IActionResult> CurrentCourses()
    {
        var teacher = await GetCurrentTeacher();
        if (teacher is null) return NotFound("Teacher profile was not found.");

        return View(new TeacherCoursesViewModel
        {
            TeacherName = User.Identity?.Name ?? "Teacher",
            Courses = await _teacherService.GetTeacherCurrentCourses(teacher.TeacherId)
        });
    }

    [HttpGet]
    public async Task<IActionResult> CourseStudents(int id)
    {
        var teacher = await GetCurrentTeacher();
        if (teacher is null) return NotFound("Teacher profile was not found.");

        var course = await _teacherService.GetTeacherCourseWithStudents(teacher.TeacherId, id);
        if (course is null) return NotFound("This course is not assigned to you.");

        return View(new TeacherCourseStudentsViewModel { Course = course });
    }

    [HttpGet]
    public async Task<IActionResult> UpdateGrade(int enrollmentId)
    {
        var teacher = await GetCurrentTeacher();
        if (teacher is null) return NotFound("Teacher profile was not found.");

        var enrollment = await _context.Enrollments
            .Include(e => e.Student).ThenInclude(s => s.AppUser)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId &&
                e.Course.CourseTeachers.Any(ct => ct.TeacherId == teacher.TeacherId));

        if (enrollment is null) return NotFound("This enrollment is not in one of your courses.");

        return View(new UpdateGradeViewModel
        {
            EnrollmentId = enrollment.EnrollmentId,
            CourseId = enrollment.CourseId,
            StudentName = enrollment.Student.AppUser.FullName,
            CourseName = enrollment.Course.CourseName,
            Grade = enrollment.Grade
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateGrade(UpdateGradeViewModel model)
    {
        var teacher = await GetCurrentTeacher();
        if (teacher is null) return NotFound("Teacher profile was not found.");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _teacherService.UpdateGrade(teacher.TeacherId, model.EnrollmentId, model.Grade);
        if (!updated)
        {
            ModelState.AddModelError(string.Empty, "The grade could not be updated. Check that the enrollment belongs to your course and the grade is between 0 and 100.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Grade updated successfully.";
        return RedirectToAction(nameof(CourseStudents), new { id = model.CourseId });
    }

    private async Task<Teacher?> GetCurrentTeacher()
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdText, out var userId)
            ? await _teacherService.GetTeacherByUserId(userId)
            : null;
    }
}

public class TeacherDashboardViewModel
{
    public string TeacherName { get; set; } = string.Empty;
    public int CourseCount { get; set; }
}
