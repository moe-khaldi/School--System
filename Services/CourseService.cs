using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.Enums;

namespace UniversityCourseEnrollment.Services;

public class CourseService
{
    private readonly ApplicationDbContext _context;

    public CourseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetCourseDetails(int courseId)
    {
        return await _context.Courses
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
                    .ThenInclude(s => s.AppUser)
            .Include(c => c.CourseTeachers)
                .ThenInclude(ct => ct.Teacher)
                    .ThenInclude(t => t.AppUser)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);
    }

    public async Task<decimal?> GetCourseAverage(int courseId)
    {
        var grades = await _context.Enrollments
            .Where(e => e.CourseId == courseId && e.Grade.HasValue)
            .Select(e => e.Grade!.Value)
            .ToListAsync();

        return grades.Count == 0 ? null : Math.Round(grades.Average(), 2);
    }

    public async Task<bool> AssignTeacherToCourse(int teacherId, int courseId)
    {
        var exists = await _context.CourseTeachers
            .AnyAsync(x => x.TeacherId == teacherId && x.CourseId == courseId);
        if (exists) return false;

        _context.CourseTeachers.Add(new CourseTeacher
        {
            TeacherId = teacherId,
            CourseId = courseId,
            AssignedDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Cancelled, int ActiveEnrollmentCount)> CancelCourseIfLowEnrollment(int courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.CourseId == courseId);

        if (course is null) return (false, -1);

        var activeEnrollments = course.Enrollments
            .Where(e => e.Status == EnrollmentStatus.Enrolled)
            .ToList();

        if (activeEnrollments.Count >= 5) return (false, activeEnrollments.Count);

        course.IsCancelled = true;
        course.IsActive = false;
        foreach (var enrollment in activeEnrollments)
            enrollment.Status = EnrollmentStatus.Cancelled;

        await _context.SaveChangesAsync();
        return (true, activeEnrollments.Count);
    }
}
