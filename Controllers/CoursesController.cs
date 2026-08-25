using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;

namespace UniversityCourseEnrollment.Controllers;

[Authorize(Roles = "Admin")]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses
            .Include(c => c.CourseTeachers)
                .ThenInclude(ct => ct.Teacher)
                    .ThenInclude(t => t.AppUser)
            .Include(c => c.Enrollments)
            .OrderBy(c => c.CourseCode)
            .ToListAsync();

        return View(courses);
    }
}
