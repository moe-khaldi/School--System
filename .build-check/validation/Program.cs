using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using UniversityCourseEnrollment.Controllers;
using UniversityCourseEnrollment.Data;
using UniversityCourseEnrollment.Models;
using UniversityCourseEnrollment.Models.ViewModels;
using UniversityCourseEnrollment.Services;

var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(config.GetConnectionString("DefaultConnection")).Options);
if (args.Contains("--course-status"))
{
    var today = DateTime.UtcNow.Date;
    foreach (var row in await db.Courses.AsNoTracking().OrderBy(c => c.CourseId).Select(c => new { c.CourseId, c.CourseCode, c.IsActive, c.IsCancelled, c.EndDate }).ToListAsync())
        Console.WriteLine($"ID={row.CourseId} Code={row.CourseCode} Active={row.IsActive} Cancelled={row.IsCancelled} End={row.EndDate:yyyy-MM-dd} ShowActivate={!row.IsActive && !row.IsCancelled && row.EndDate >= today}");
    return;
}
await using var transaction = await db.Database.BeginTransactionAsync();
var count = 0;
void Check(bool success, string name) { if (!success) throw new Exception(name); Console.WriteLine("PASS: " + name); count++; }
bool Valid(object model) => Validator.TryValidateObject(model, new ValidationContext(model), new List<ValidationResult>(), true);
Check(!Valid(new CreateCourseViewModel()), "Required course fields");
Check(!Valid(new CreateCourseViewModel { CourseCode = "T", CourseName = "Test", StartDate = null }), "Missing date");
Check(!Valid(new CreateCourseViewModel { CourseCode = "T", CourseName = "Test", EndDate = DateTime.Today }), "Date ordering");
Check(!Valid(new CreateCourseViewModel { CourseCode = "T", CourseName = "Test", CreditHours = 7 }), "Credit hours range");
Check(!Valid(new AssignTeacherViewModel { CourseId = 0, TeacherId = -1 }), "Nonpositive assignment IDs");
var service = new CourseService(db);
Check(!(await service.AssignTeacherToCourse(0, 0)).Success, "Service rejects invalid IDs");
Check(!(await service.AssignTeacherToCourse(int.MaxValue, 1)).Success, "Service rejects missing teacher");
var teacher = await db.Teachers.FirstAsync();
Check(!(await service.AssignTeacherToCourse(teacher.TeacherId, int.MaxValue)).Success, "Service rejects missing course");
var controller = new AdminController(db, new GpaService(db), service) { TempData = new TempDataDictionary(new DefaultHttpContext(), new MemoryTempData()) };
var model = new CreateCourseViewModel { CourseCode = "V" + Guid.NewGuid().ToString("N")[..12], CourseName = " Validation check " };
Check(await controller.CreateCourse(model) is RedirectToActionResult, "Valid course creation");
var course = await db.Courses.SingleAsync(c => c.CourseCode == model.CourseCode);
Check(!course.IsActive && !course.IsCancelled && course.CourseName == "Validation check", "Inactive default and trimmed name");
Check(!(await service.ActivateCourse(int.MaxValue)).Success, "Activation rejects missing course");
Check((await service.ActivateCourse(course.CourseId)).Success, "Activate inactive course");
await db.Entry(course).ReloadAsync();
Check(course.IsActive, "Activation persisted");
Check((await service.ActivateCourse(course.CourseId)).Success, "Repeated activation succeeds safely");
course.IsActive = false;
course.IsCancelled = true;
await db.SaveChangesAsync();
Check(!(await service.ActivateCourse(course.CourseId)).Success, "Activation rejects cancelled course");
course.IsCancelled = false;
course.StartDate = DateTime.UtcNow.Date.AddDays(-2);
course.EndDate = DateTime.UtcNow.Date.AddDays(-1);
await db.SaveChangesAsync();
Check(!(await service.ActivateCourse(course.CourseId)).Success, "Activation rejects expired course");
course.StartDate = DateTime.UtcNow.Date.AddDays(1);
course.EndDate = DateTime.UtcNow.Date.AddDays(30);
await db.SaveChangesAsync();
Check((await service.ActivateCourse(course.CourseId)).Success, "Future course can be activated");
await db.Entry(course).ReloadAsync();
Check(!(await new EnrollmentService(db).GetOpenCourses()).Any(c => c.CourseId == course.CourseId), "Future active course stays closed for enrollment");
Check(await controller.CreateCourse(model) is ViewResult && controller.ModelState.ContainsKey("CourseCode"), "Friendly duplicate course validation");
Check((await service.AssignTeacherToCourse(teacher.TeacherId, course.CourseId)).Success, "Valid teacher assignment");
Check(!(await service.AssignTeacherToCourse(teacher.TeacherId, course.CourseId)).Success, "Duplicate assignment rejected");
foreach (var sql in new[] {
    "UPDATE Courses SET CreditHours = 7 WHERE CourseId = {0}",
    "UPDATE Courses SET EndDate = StartDate WHERE CourseId = {0}" })
{
    var rejected = false;
    try { await db.Database.ExecuteSqlRawAsync(sql, course.CourseId); }
    catch (SqlException ex) when (ex.Number == 547) { rejected = true; }
    Check(rejected, "Database rejects " + sql.Split(" SET ")[1].Split(" WHERE ")[0]);
}
var enrollment = await db.Enrollments.FirstAsync();
foreach (var sql in new[] {
    "UPDATE Enrollments SET Grade = 101 WHERE EnrollmentId = {0}",
    "UPDATE Enrollments SET EnrollmentEndDate = EnrollmentStartDate WHERE EnrollmentId = {0}" })
{
    var rejected = false;
    try { await db.Database.ExecuteSqlRawAsync(sql, enrollment.EnrollmentId); }
    catch (SqlException ex) when (ex.Number == 547) { rejected = true; }
    Check(rejected, "Database rejects " + sql.Split(" SET ")[1].Split(" WHERE ")[0]);
}
var teacherService = new TeacherService(db);
Check(!await teacherService.UpdateGrade(teacher.TeacherId, enrollment.EnrollmentId, 101), "Grade service rejects invalid grade");
Check(!await teacherService.UpdateGrade(int.MaxValue, enrollment.EnrollmentId, 50), "Grade service rejects unauthorized teacher");
var enrollmentService = new EnrollmentService(db);
Check(!(await enrollmentService.EnrollStudentInCourse(enrollment.StudentId, enrollment.CourseId)).Success, "Enrollment service rejects duplicate");
await transaction.RollbackAsync();
Console.WriteLine($"All {count} checks passed; test writes rolled back.");

class MemoryTempData : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
