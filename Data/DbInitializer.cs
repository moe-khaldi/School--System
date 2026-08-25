using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.Enums;

namespace UniversityCourseEnrollment.Data;

/// <summary>
/// Creates predictable demo data for local learning and manual testing.
/// SQL Server generates all identity keys; navigation properties connect the rows.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.AppUsers.AnyAsync())
            return;

        var today = DateTime.UtcNow.Date;

        var users = new List<AppUser>
        {
            new() { FullName = "System Administrator", Email = "admin@university.local", Password = "Admin123!", Role = UserRole.Admin },
            new() { FullName = "Dr. Lina Haddad", Email = "lina.haddad@university.local", Password = "Teacher123!", Role = UserRole.Teacher },
            new() { FullName = "Dr. Omar Saleh", Email = "omar.saleh@university.local", Password = "Teacher123!", Role = UserRole.Teacher },
            new() { FullName = "Dr. Noor Khalil", Email = "noor.khalil@university.local", Password = "Teacher123!", Role = UserRole.Teacher }
        };

        for (var i = 1; i <= 15; i++)
        {
            users.Add(new AppUser
            {
                FullName = $"Student {i:00}",
                Email = $"student{i:00}@university.local",
                Password = "Student123!",
                Role = UserRole.Student
            });
        }

        var teachers = new List<Teacher>
        {
            new() { AppUser = users[1], Department = "Computer Science" },
            new() { AppUser = users[2], Department = "Information Technology" },
            new() { AppUser = users[3], Department = "Software Engineering" }
        };

        var students = Enumerable.Range(1, 15)
            .Select(i => new Student { AppUser = users[i + 3], UniversityNumber = $"U2026{i:000}" })
            .ToList();

        var courses = new List<Course>
        {
            new() { CourseCode = "CS101", CourseName = "Introduction to Programming", Description = "Programming fundamentals with C#.", CreditHours = 3, StartDate = today.AddDays(-60), EndDate = today.AddDays(120), IsActive = true },
            new() { CourseCode = "DB201", CourseName = "Database Systems", Description = "Relational design, SQL, and Entity Framework Core.", CreditHours = 3, StartDate = today.AddDays(-45), EndDate = today.AddDays(100), IsActive = true },
            new() { CourseCode = "WEB301", CourseName = "Web Application Development", Description = "MVC web applications and web fundamentals.", CreditHours = 3, StartDate = today.AddDays(-10), EndDate = today.AddDays(90), IsActive = true },
            new() { CourseCode = "AI401", CourseName = "Applied Artificial Intelligence", Description = "An introduction to practical AI concepts.", CreditHours = 4, StartDate = today.AddDays(30), EndDate = today.AddDays(150), IsActive = true },
            new() { CourseCode = "MATH101", CourseName = "Discrete Mathematics", Description = "Logic, sets, graphs, and proofs.", CreditHours = 3, StartDate = today.AddDays(-300), EndDate = today.AddDays(-150), IsActive = false },
            new() { CourseCode = "NET201", CourseName = "Computer Networks", Description = "Network protocols and communication models.", CreditHours = 3, StartDate = today.AddDays(-20), EndDate = today.AddDays(60), IsActive = true },
            new() { CourseCode = "SE202", CourseName = "Software Design", Description = "Design principles and maintainable software.", CreditHours = 3, StartDate = today.AddDays(-250), EndDate = today.AddDays(-120), IsActive = false, IsCancelled = true },
            new() { CourseCode = "UX101", CourseName = "User Experience Basics", Description = "Usability and user-centered design.", CreditHours = 2, StartDate = today.AddDays(50), EndDate = today.AddDays(150), IsActive = true }
        };

        var assignments = new List<CourseTeacher>
        {
            new() { Teacher = teachers[0], Course = courses[0] },
            new() { Teacher = teachers[0], Course = courses[1] },
            new() { Teacher = teachers[0], Course = courses[3] },
            new() { Teacher = teachers[1], Course = courses[0] },
            new() { Teacher = teachers[1], Course = courses[2] },
            new() { Teacher = teachers[1], Course = courses[4] },
            new() { Teacher = teachers[1], Course = courses[5] },
            new() { Teacher = teachers[2], Course = courses[2] },
            new() { Teacher = teachers[2], Course = courses[7] }
        };

        var enrollments = new List<Enrollment>();

        // CS101: eight enrollments with completed grades for GPA and average tests.
        AddEnrollment(1, 1, EnrollmentStatus.Completed, 95, -55);
        AddEnrollment(2, 1, EnrollmentStatus.Completed, 88, -54);
        AddEnrollment(3, 1, EnrollmentStatus.Completed, 76, -53);
        AddEnrollment(4, 1, EnrollmentStatus.Completed, 64, -52);
        AddEnrollment(5, 1, EnrollmentStatus.Enrolled, null, -10);
        AddEnrollment(6, 1, EnrollmentStatus.Enrolled, null, -9);
        AddEnrollment(7, 1, EnrollmentStatus.Enrolled, null, -8);
        AddEnrollment(8, 1, EnrollmentStatus.Enrolled, null, -7);

        // DB201: three active students, so it is eligible for cancellation.
        AddEnrollment(9, 2, EnrollmentStatus.Enrolled, null, -40);
        AddEnrollment(10, 2, EnrollmentStatus.Enrolled, null, -39);
        AddEnrollment(11, 2, EnrollmentStatus.Enrolled, null, -38);

        // WEB301: current course with one completed grade and active students.
        AddEnrollment(12, 3, EnrollmentStatus.Completed, 91, -8);
        AddEnrollment(13, 3, EnrollmentStatus.Enrolled, null, -7);
        AddEnrollment(14, 3, EnrollmentStatus.Enrolled, null, -6);
        AddEnrollment(15, 3, EnrollmentStatus.Enrolled, null, -5);

        // NET201: exactly five active students, so cancellation must be rejected.
        AddEnrollment(1, 6, EnrollmentStatus.Enrolled, null, -15);
        AddEnrollment(2, 6, EnrollmentStatus.Enrolled, null, -14);
        AddEnrollment(3, 6, EnrollmentStatus.Enrolled, null, -13);
        AddEnrollment(4, 6, EnrollmentStatus.Enrolled, null, -12);
        AddEnrollment(5, 6, EnrollmentStatus.Enrolled, null, -11);

        // Historical grades add more GPA and honor-board records.
        AddEnrollment(6, 5, EnrollmentStatus.Completed, 98, -295);
        AddEnrollment(7, 5, EnrollmentStatus.Completed, 82, -294);

        // Cancelled history is retained rather than deleted.
        AddEnrollment(8, 7, EnrollmentStatus.Cancelled, null, -245);
        AddEnrollment(9, 7, EnrollmentStatus.Cancelled, null, -244);

        // Future enrollments demonstrate that future courses are not currently open.
        AddEnrollment(10, 4, EnrollmentStatus.Enrolled, null, 30);
        AddEnrollment(11, 8, EnrollmentStatus.Enrolled, null, 50);

        await using var transaction = await context.Database.BeginTransactionAsync();
        context.AddRange(users);
        context.AddRange(teachers);
        context.AddRange(students);
        context.AddRange(courses);
        context.AddRange(assignments);
        context.AddRange(enrollments);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        void AddEnrollment(int studentNumber, int courseNumber, EnrollmentStatus status, decimal? grade, int daysFromToday)
        {
            var course = courses[courseNumber - 1];
            enrollments.Add(new Enrollment
            {
                Student = students[studentNumber - 1],
                Course = course,
                EnrollmentDate = today.AddDays(daysFromToday),
                EnrollmentStartDate = course.StartDate,
                EnrollmentEndDate = course.EndDate,
                Status = status,
                Grade = grade
            });
        }
    }
}
