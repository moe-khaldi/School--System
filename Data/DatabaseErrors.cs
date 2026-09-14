using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace UniversityCourseEnrollment.Data;

internal static class DatabaseErrors
{
    public static bool IsDuplicate(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    public static bool IsConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 547 };
}
