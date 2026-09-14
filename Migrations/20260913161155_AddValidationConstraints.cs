using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversityCourseEnrollment.Migrations
{
    /// <inheritdoc />
    public partial class AddValidationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollments_Dates",
                table: "Enrollments",
                sql: "[EnrollmentEndDate] > [EnrollmentStartDate]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Enrollments_Grade",
                table: "Enrollments",
                sql: "[Grade] IS NULL OR [Grade] BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Courses_CreditHours",
                table: "Courses",
                sql: "[CreditHours] BETWEEN 1 AND 6");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Courses_Dates",
                table: "Courses",
                sql: "[EndDate] > [StartDate]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollments_Dates",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Enrollments_Grade",
                table: "Enrollments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Courses_CreditHours",
                table: "Courses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Courses_Dates",
                table: "Courses");
        }
    }
}
