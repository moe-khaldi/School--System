using Microsoft.EntityFrameworkCore;
using UniversityCourseEnrollment.Models;

namespace UniversityCourseEnrollment.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<CourseTeacher> CourseTeachers => Set<CourseTeacher>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(x => x.UniversityNumber).IsUnique();
        modelBuilder.Entity<Course>().HasIndex(x => x.CourseCode).IsUnique();
        modelBuilder.Entity<Enrollment>().HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();
        modelBuilder.Entity<CourseTeacher>().HasIndex(x => new { x.TeacherId, x.CourseId }).IsUnique();

        modelBuilder.Entity<AppUser>().Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        modelBuilder.Entity<Enrollment>().Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        modelBuilder.Entity<AppUser>().HasOne(x => x.Student).WithOne(x => x.AppUser)
            .HasForeignKey<Student>(x => x.AppUserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AppUser>().HasOne(x => x.Teacher).WithOne(x => x.AppUser)
            .HasForeignKey<Teacher>(x => x.AppUserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>().HasOne(x => x.Student).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrollment>().HasOne(x => x.Course).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseTeacher>().HasOne(x => x.Teacher).WithMany(x => x.CourseTeachers)
            .HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CourseTeacher>().HasOne(x => x.Course).WithMany(x => x.CourseTeachers)
            .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>().Property(x => x.CreditHours).IsRequired();
        modelBuilder.Entity<Enrollment>().Property(x => x.Grade).HasPrecision(5, 2);
    }
}
