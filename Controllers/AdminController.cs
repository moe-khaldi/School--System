using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.ViewModels;
using UniversityCourseEnrollment.Services;

namespace UniversityCourseEnrollment.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly GpaService _gpaService;
    private readonly CourseService _courseService;

    public AdminController(ApplicationDbContext context, GpaService gpaService, CourseService courseService)
    {
        _context = context;
        _gpaService = gpaService;
        _courseService = courseService;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(Dashboard));

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var model = new AdminDashboardViewModel
        {
            CourseCount = await _context.Courses.CountAsync(),
            TeacherCount = await _context.Teachers.CountAsync(),
            StudentCount = await _context.Students.CountAsync(),
            EnrollmentCount = await _context.Enrollments.CountAsync()
        };

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Teachers(string? search, int pageNumber = 1,
        int pageSize = 10, CancellationToken cancellationToken = default)
    {
        search = search?.Trim();
        ViewData["Search"] = search;
        IQueryable<Teacher> query = _context.Teachers.AsNoTracking()
            .Include(t => t.AppUser).Include(t => t.CourseTeachers);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => t.AppUser.FullName.Contains(search) || t.AppUser.Email.Contains(search) || (t.Department != null && t.Department.Contains(search)));

        return View(await PaginatedList<Teacher>.CreateAsync(
            query.OrderBy(t => t.AppUser.FullName).ThenBy(t => t.TeacherId),
            pageNumber, pageSize, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Students(string? search, int pageNumber = 1,
        int pageSize = 10, CancellationToken cancellationToken = default)
    {
        search = search?.Trim();
        ViewData["Search"] = search;
        IQueryable<Student> query = _context.Students.AsNoTracking()
            .Include(s => s.AppUser).Include(s => s.Enrollments);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(s => s.AppUser.FullName.Contains(search) || s.UniversityNumber.Contains(search) || s.AppUser.Email.Contains(search));

        return View(await PaginatedList<Student>.CreateAsync(
            query.OrderBy(s => s.AppUser.FullName).ThenBy(s => s.StudentId),
            pageNumber, pageSize, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Enrollments(string? search, int pageNumber = 1,
        int pageSize = 10, CancellationToken cancellationToken = default)
    {
        search = search?.Trim();
        ViewData["Search"] = search;
        IQueryable<Enrollment> query = _context.Enrollments.AsNoTracking()
            .Include(e => e.Student).ThenInclude(s => s.AppUser).Include(e => e.Course);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(e => e.Student.AppUser.FullName.Contains(search) || e.Course.CourseCode.Contains(search) || e.Course.CourseName.Contains(search));

        return View(await PaginatedList<Enrollment>.CreateAsync(
            query.OrderByDescending(e => e.EnrollmentDate).ThenBy(e => e.EnrollmentId),
            pageNumber, pageSize, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> CreateCourse()
    {
        return View(new Course
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(3),
            CreditHours = 3
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(Course course)
    {
        if (!ModelState.IsValid) return View(course);

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Course created successfully.";
        return RedirectToAction(nameof(CourseDetails), new { id = course.CourseId });
    }

    [HttpGet]
    public async Task<IActionResult> CourseDetails(int id, int pageNumber = 1,
        int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses.AsNoTracking()
            .Include(c => c.CourseTeachers)
                .ThenInclude(ct => ct.Teacher)
                    .ThenInclude(t => t.AppUser)
            .FirstOrDefaultAsync(c => c.CourseId == id, cancellationToken);
        if (course is null) return NotFound();

        var enrollments = _context.Enrollments.AsNoTracking()
            .Where(e => e.CourseId == id)
            .Include(e => e.Student).ThenInclude(s => s.AppUser)
            .OrderBy(e => e.Student.AppUser.FullName).ThenBy(e => e.EnrollmentId);

        return View(new CourseDetailsViewModel
        {
            Course = course,
            AverageGrade = await _courseService.GetCourseAverage(id),
            Enrollments = await PaginatedList<Enrollment>.CreateAsync(
                enrollments, pageNumber, pageSize, cancellationToken)
        });
    }

    [HttpGet]
    public async Task<IActionResult> AssignTeacher(int? courseId)
    {
        ViewBag.Courses = await _context.Courses.OrderBy(c => c.CourseCode).ToListAsync();
        ViewBag.Teachers = await _context.Teachers.Include(t => t.AppUser).OrderBy(t => t.AppUser.FullName).ToListAsync();
        return View(new AssignTeacherViewModel { CourseId = courseId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignTeacher(AssignTeacherViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = await _context.Courses.OrderBy(c => c.CourseCode).ToListAsync();
            ViewBag.Teachers = await _context.Teachers.Include(t => t.AppUser).OrderBy(t => t.AppUser.FullName).ToListAsync();
            return View(model);
        }

        var assigned = await _courseService.AssignTeacherToCourse(model.TeacherId, model.CourseId);
        TempData[assigned ? "SuccessMessage" : "ErrorMessage"] = assigned
            ? "Teacher assigned successfully."
            : "This teacher is already assigned to this course.";
        return RedirectToAction(nameof(CourseDetails), new { id = model.CourseId });
    }

    [HttpGet]
    public async Task<IActionResult> StudentGpa(int? studentId)
    {
        var students = await _context.Students.Include(s => s.AppUser).OrderBy(s => s.AppUser.FullName).ToListAsync();
        ViewBag.Students = students;
        if (!studentId.HasValue) return View(new StudentGpaViewModel());

        var student = students.FirstOrDefault(s => s.StudentId == studentId.Value);
        if (student is null) return NotFound();
        return View(new StudentGpaViewModel
        {
            StudentId = student.StudentId,
            StudentName = student.AppUser.FullName,
            UniversityNumber = student.UniversityNumber,
            Gpa = await _gpaService.CalculateStudentGpa(student.StudentId)
        });
    }

    [HttpGet]
    public async Task<IActionResult> CourseAverage(int id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);
        if (course is null) return NotFound();
        return View(new CourseDetailsViewModel { Course = course, AverageGrade = await _courseService.GetCourseAverage(id) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelCourseIfLowEnrollment(int id)
    {
        var result = await _courseService.CancelCourseIfLowEnrollment(id);
        if (result.Cancelled)
            TempData["SuccessMessage"] = $"Course cancelled. {result.ActiveEnrollmentCount} active enrollments were marked Cancelled.";
        else if (result.ActiveEnrollmentCount >= 0)
            TempData["ErrorMessage"] = $"Course cannot be cancelled because it has {result.ActiveEnrollmentCount} active enrollments.";
        else
            TempData["ErrorMessage"] = "Course was not found.";

        return RedirectToAction(nameof(CourseDetails), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> HonorBoard()
    {
        return View(await _gpaService.GetTop10StudentsByGpa());
    }

    [HttpGet]
    public async Task<IActionResult> StudentEnrollments(int id, DateTime? fromDate, DateTime? toDate)
    {
        var student = await _context.Students.Include(s => s.AppUser).FirstOrDefaultAsync(s => s.StudentId == id);
        if (student is null) return NotFound();

        var query = _context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == id);
        if (fromDate.HasValue) query = query.Where(e => e.EnrollmentStartDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(e => e.EnrollmentEndDate <= toDate.Value);

        return View(new StudentEnrollmentsReportViewModel
        {
            StudentId = id,
            StudentName = student.AppUser.FullName,
            FromDate = fromDate,
            ToDate = toDate,
            Enrollments = await query.OrderBy(e => e.EnrollmentStartDate).ToListAsync()
        });
    }

    [HttpGet]
    public async Task<IActionResult> TeacherCurrentCourses(int id)
    {
        var teacher = await _context.Teachers.Include(t => t.AppUser).FirstOrDefaultAsync(t => t.TeacherId == id);
        if (teacher is null) return NotFound();

        var today = DateTime.UtcNow.Date;
        var courses = await _context.CourseTeachers
            .Where(ct => ct.TeacherId == id && !ct.Course.IsCancelled && ct.Course.StartDate <= today && ct.Course.EndDate >= today)
            .Select(ct => ct.Course)
            .OrderBy(c => c.CourseCode)
            .ToListAsync();

        return View(new TeacherCurrentCoursesViewModel { TeacherName = teacher.AppUser.FullName, Courses = courses });
    }
}

public class AdminDashboardViewModel
{
    public int CourseCount { get; set; }
    public int TeacherCount { get; set; }
    public int StudentCount { get; set; }
    public int EnrollmentCount { get; set; }
}
