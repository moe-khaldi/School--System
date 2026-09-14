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

    public async Task<(bool Success, string Message)> AssignTeacherToCourse(int teacherId, int courseId)
    {
        if (teacherId <= 0 || !await _context.Teachers.AnyAsync(t => t.TeacherId == teacherId))
            return (false, "Choose an existing teacher.");
        if (courseId <= 0 || !await _context.Courses.AnyAsync(c => c.CourseId == courseId))
            return (false, "Choose an existing course.");

        var exists = await _context.CourseTeachers
            .AnyAsync(x => x.TeacherId == teacherId && x.CourseId == courseId);
        if (exists) return (false, "This teacher is already assigned to this course.");

        var assignment = new CourseTeacher
        {
            TeacherId = teacherId,
            CourseId = courseId,
            AssignedDate = DateTime.UtcNow
        };
        _context.CourseTeachers.Add(assignment);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (DatabaseErrors.IsDuplicate(ex))
        {
            _context.Entry(assignment).State = EntityState.Detached;
            return (false, "This teacher is already assigned to this course.");
        }
        catch (DbUpdateException ex) when (DatabaseErrors.IsConstraintViolation(ex))
        {
            _context.Entry(assignment).State = EntityState.Detached;
            return (false, "The teacher or course is no longer available. Refresh and try again.");
        }
        return (true, "Teacher assigned successfully.");
    }

    public async Task<(bool Success, string Message)> ActivateCourse(int courseId)
    {
        var today = DateTime.UtcNow.Date;
        // Check eligibility in the update itself so a concurrent cancellation cannot be undone.
        var changed = await _context.Courses
            .Where(c => c.CourseId == courseId && !c.IsActive && !c.IsCancelled && c.EndDate >= today)
            .ExecuteUpdateAsync(update => update.SetProperty(c => c.IsActive, true));
        if (changed > 0) return (true, "Course activated successfully.");

        var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.CourseId == courseId);
        if (course is null) return (false, "The course was not found.");
        if (course.IsCancelled) return (false, "A cancelled course cannot be activated.");
        if (course.EndDate < today) return (false, "A course that has ended cannot be activated.");
        return (true, "The course is already active.");
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
