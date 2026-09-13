using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.ViewModels;

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
    public async Task<IActionResult> Index(string? search, int pageNumber = 1,
        int pageSize = 10, CancellationToken cancellationToken = default)
    {
        search = search?.Trim();
        ViewData["Search"] = search;
        IQueryable<Course> query = _context.Courses.AsNoTracking()
            .Include(c => c.Enrollments);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.CourseCode.Contains(search) || c.CourseName.Contains(search));

        return View(await PaginatedList<Course>.CreateAsync(
            query.OrderBy(c => c.CourseCode).ThenBy(c => c.CourseId),
            pageNumber, pageSize, cancellationToken));
    }
}
