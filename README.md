# University Course Enrollment MVC System 

##System Details
-This system uses a .NET MVC framework with razor view pages.
-System has 3 User types: Admin, Teacher and Student. Each user type has its own privilages and view pages.Teacher and student have user IDs , users can only be one of the 3.
-Admin role can:
 -Admin can get student GPA
 -Admin can get course details including average score for it
 -Admin can see how many courses the teacher is assigned
 -Admin can retrieve a timeframe where he can see what courses the student is enrolled in
 -Admin can drop courses if students enrolled are less than 5.
 -Admin can know the top 10 student GPAs and their highest course grade
 -Admin can create course
 -Admin can assign teachers a course

-Teacher role can: 
 -Teacher can be assigned many courses
 -Teacher  can see the courses admin added for him, and the students enrolled in these courses
 
 


## Tables and keys

- **AppUser** (`AppUserId` primary key) stores the common identity data and one role: Admin, Teacher, or Student. `Email` is unique.
- **Student** (`StudentId` primary key) stores student-specific data. `AppUserId` is a foreign key to `AppUser`, and `UniversityNumber` is unique.
- **Teacher** (`TeacherId` primary key) stores teacher-specific data. `AppUserId` is a foreign key to `AppUser`.
- **Course** (`CourseId` primary key) stores course details, dates, status, and credit hours. `CourseCode` is unique.
- **Enrollment** (`EnrollmentId` primary key) connects a student to a course and stores enrollment dates, status, and grade. `(StudentId, CourseId)` is a unique composite key.
- **CourseTeacher** (`CourseTeacherId` primary key) connects teachers to courses. `(TeacherId, CourseId)` is a unique composite key.

## Relationships

- AppUser 1 -> 0/1 Student
- AppUser 1 -> 0/1 Teacher
- Student 1 -> many Enrollments
- Course 1 -> many Enrollments
- Teacher 1 -> many CourseTeachers
- Course 1 -> many CourseTeachers

Enrollment is required because a student-to-course relationship has its own data: dates, status, and grade. CourseTeacher is required because teachers and courses are many-to-many and the assignment has an `AssignedDate`.

## Business rules and queries

- Enrollments are never deleted when a student drops or a course is cancelled. The status changes to `Dropped` or `Cancelled`, preserving academic history.
- GPA uses completed enrollments with grades and credit-hour weighting: `sum(grade points × credit hours) / sum(credit hours)`. Grade points follow the scale defined in `GpaService`.
- Course average is the arithmetic average of non-null grades for that course.
- Current teacher courses require `StartDate <= today`, `EndDate >= today`, and `IsCancelled = false`, plus a matching CourseTeacher row.
- Duplicate enrollments and duplicate teacher assignments are prevented by composite unique indexes in addition to application checks.
- A low-enrollment course counts active enrollments. If fewer than five students are enrolled, the course is marked cancelled/inactive and its active enrollments become `Cancelled`; otherwise it remains unchanged.
- Course date ordering and grade range are validated by model validation, while relationships, foreign keys, required fields, precision, and uniqueness are configured with EF Core Fluent API.
