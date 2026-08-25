using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.Enums;

namespace UniversityCourseEnrollment.Services;

public class EnrollmentService
{
    private readonly ApplicationDbContext _context;

    public EnrollmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Course>> GetOpenCourses()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Courses
            .Where(c => c.IsActive && !c.IsCancelled && c.StartDate <= today && c.EndDate >= today)
            .Include(c => c.Enrollments)
            .OrderBy(c => c.CourseCode)
            .ToListAsync();
    }

    public async Task<List<Enrollment>> GetStudentEnrollments(int studentId)
    {
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Course)
            .OrderByDescending(e => e.EnrollmentDate)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> EnrollStudentInCourse(int studentId, int courseId)
    {
        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        if (alreadyEnrolled)
            return (false, "You already have an enrollment record for this course.");

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
        if (course is null)
            return (false, "The course was not found.");

        var today = DateTime.UtcNow.Date;
        if (!course.IsActive || course.IsCancelled)
            return (false, "You cannot enroll in an inactive or cancelled course.");
        if (today < course.StartDate.Date || today > course.EndDate.Date)
            return (false, "You can only enroll while the course is within its valid dates.");

        _context.Enrollments.Add(new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            EnrollmentStartDate = course.StartDate,
            EnrollmentEndDate = course.EndDate,
            Status = EnrollmentStatus.Enrolled
        });

        await _context.SaveChangesAsync();
        return (true, "You enrolled in the course successfully.");
    }
}
