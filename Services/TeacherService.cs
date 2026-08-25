using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.Enums;

namespace UniversityCourseEnrollment.Services;

public class TeacherService
{
    private readonly ApplicationDbContext _context;

    public TeacherService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Teacher?> GetTeacherByUserId(int appUserId)
    {
        return await _context.Teachers.FirstOrDefaultAsync(t => t.AppUserId == appUserId);
    }

    public async Task<List<Course>> GetTeacherCourses(int teacherId)
    {
        var courseTeachers = await _context.CourseTeachers
            .Include(ct => ct.Course)
                .ThenInclude(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s.AppUser)
            .Include(ct => ct.Teacher)
                .ThenInclude(t => t.AppUser)
            .Where(ct => ct.TeacherId == teacherId)
            .OrderBy(ct => ct.Course.CourseCode)
            .ToListAsync();

        return courseTeachers
            .Select(ct => ct.Course)
            .ToList();
    }

    public async Task<List<Course>> GetTeacherCurrentCourses(int teacherId)
    {
        var today = DateTime.UtcNow.Date;
        var courseTeachers = await _context.CourseTeachers
            .Include(ct => ct.Course)
                .ThenInclude(c => c.Enrollments)
            .Where(ct => ct.TeacherId == teacherId &&
                         !ct.Course.IsCancelled &&
                         ct.Course.StartDate <= today &&
                         ct.Course.EndDate >= today)
            .OrderBy(ct => ct.Course.CourseCode)
            .ToListAsync();

        return courseTeachers.Select(ct => ct.Course).ToList();
    }

    public async Task<Course?> GetTeacherCourseWithStudents(int teacherId, int courseId)
    {
        var assignment = await _context.CourseTeachers
            .Include(ct => ct.Course)
                .ThenInclude(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s.AppUser)
            .FirstOrDefaultAsync(ct => ct.TeacherId == teacherId && ct.CourseId == courseId);

        return assignment?.Course;
    }

    public async Task<int> GetTeacherCourseCount(int teacherId)
    {
        return await _context.CourseTeachers.CountAsync(ct => ct.TeacherId == teacherId);
    }

    public async Task<bool> UpdateGrade(int teacherId, int enrollmentId, decimal? grade)
    {
        if (grade is < 0 or > 100) return false;

        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId &&
                e.Course.CourseTeachers.Any(ct => ct.TeacherId == teacherId));

        if (enrollment is null) return false;

        enrollment.Grade = grade;
        enrollment.Status = grade.HasValue
            ? EnrollmentStatus.Completed
            : EnrollmentStatus.Enrolled;

        await _context.SaveChangesAsync();
        return true;
    }
}
