using System.Data.SqlClient;

namespace AcademixPro
{
    public static class SqlQueries
    {
        public static class Auth
        {
            public const string Login =
                @"SELECT UserID, Username, FullName, Email, Role
                  FROM Users
                  WHERE Username = @Username
                    AND PasswordHash = @PasswordHash
                    AND IsActive = 1";

            public const string UpdateLastLogin =
                @"UPDATE Users SET LastLogin = GETDATE()
                  WHERE UserID = @UserID";

            public const string GetAllUsers =
                @"SELECT UserID, Username, FullName, Email, Role, IsActive, LastLogin, CreatedAt
                  FROM Users ORDER BY FullName";

            public const string InsertUser =
                @"INSERT INTO Users (Username, PasswordHash, FullName, Email, Role)
                  VALUES (@Username, @PasswordHash, @FullName, @Email, @Role)";

            public const string ToggleUserStatus =
                @"UPDATE Users SET IsActive = CASE WHEN IsActive=1 THEN 0 ELSE 1 END
                  WHERE UserID = @UserID";
        }

        public static class Students
        {
            public const string GetAll =
                @"SELECT s.StudentID, s.StudentCode, s.FullName, s.Email, s.Phone,
                         s.DateOfBirth, s.Gender, s.Address,
                         d.DeptName AS Department, s.Semester, s.CGPA,
                         s.AdmissionDate, s.IsActive
                  FROM Students s
                  LEFT JOIN Departments d ON s.DeptID = d.DeptID
                  WHERE s.IsActive = 1
                  ORDER BY s.FullName";

            public const string GetByID =
                @"SELECT s.*, d.DeptName
                  FROM Students s
                  LEFT JOIN Departments d ON s.DeptID = d.DeptID
                  WHERE s.StudentID = @StudentID";

            public const string Search =
                @"SELECT s.StudentID, s.StudentCode, s.FullName, s.Email,
                         d.DeptName AS Department, s.Semester, s.CGPA, s.IsActive
                  FROM Students s
                  LEFT JOIN Departments d ON s.DeptID = d.DeptID
                  WHERE s.IsActive = 1
                    AND (s.FullName LIKE @Term OR s.StudentCode LIKE @Term OR s.Email LIKE @Term)
                  ORDER BY s.FullName";

            public const string Insert =
                @"INSERT INTO Students
                    (StudentCode, FullName, Email, Phone, DateOfBirth, Gender, Address, DeptID, AdmissionDate, Semester)
                  VALUES
                    (@StudentCode, @FullName, @Email, @Phone, @DOB, @Gender, @Address, @DeptID, @AdmissionDate, @Semester)";

            public const string Update =
                @"UPDATE Students SET
                    FullName=@FullName, Email=@Email, Phone=@Phone,
                    DateOfBirth=@DOB, Gender=@Gender, Address=@Address,
                    DeptID=@DeptID, Semester=@Semester
                  WHERE StudentID=@StudentID";

            public const string Deactivate =
                @"UPDATE Students SET IsActive=0 WHERE StudentID=@StudentID";

            public const string GetNextCode =
                @"SELECT 'STU-' + CAST(YEAR(GETDATE()) AS NVARCHAR) + '-' +
                         RIGHT('000' + CAST(ISNULL(MAX(StudentID),0)+1 AS NVARCHAR),3)
                  FROM Students";

            public const string CountActive =
                @"SELECT COUNT(*) FROM Students WHERE IsActive=1";
        }

        public static class Instructors
        {
            public const string GetAll =
                @"SELECT i.InstructorID, i.InstructorCode, i.FullName, i.Email,
                         i.Phone, i.Specialization, d.DeptName AS Department,
                         i.JoiningDate, i.IsActive,
                         COUNT(c.CourseID) AS CoursesAssigned
                  FROM Instructors i
                  LEFT JOIN Departments d ON i.DeptID = d.DeptID
                  LEFT JOIN Courses c ON i.InstructorID = c.InstructorID AND c.IsActive=1
                  WHERE i.IsActive=1
                  GROUP BY i.InstructorID, i.InstructorCode, i.FullName, i.Email,
                           i.Phone, i.Specialization, d.DeptName, i.JoiningDate, i.IsActive
                  ORDER BY i.FullName";

            public const string Insert =
                @"INSERT INTO Instructors (InstructorCode, FullName, Email, Phone, Specialization, DeptID, JoiningDate)
                  VALUES (@Code, @FullName, @Email, @Phone, @Specialization, @DeptID, @JoiningDate)";

            public const string Update =
                @"UPDATE Instructors SET
                    FullName=@FullName, Email=@Email, Phone=@Phone,
                    Specialization=@Specialization, DeptID=@DeptID
                  WHERE InstructorID=@InstructorID";

            public const string Deactivate =
                @"UPDATE Instructors SET IsActive=0 WHERE InstructorID=@InstructorID";

            public const string GetNextCode =
                @"SELECT 'INS-' + RIGHT('000'+CAST(ISNULL(MAX(InstructorID),0)+1 AS NVARCHAR),3)
                  FROM Instructors";
        }

        public static class Courses
        {
            public const string GetAll =
                @"SELECT c.CourseID, c.CourseCode, c.CourseName, c.CreditHours,
                         d.DeptName AS Department, i.FullName AS Instructor,
                         c.Semester, c.Capacity,
                         COUNT(e.EnrollmentID) AS Enrolled,
                         c.IsActive
                  FROM Courses c
                  LEFT JOIN Departments d ON c.DeptID = d.DeptID
                  LEFT JOIN Instructors i ON c.InstructorID = i.InstructorID
                  LEFT JOIN Enrollments e ON c.CourseID = e.CourseID AND e.Status='Active'
                  WHERE c.IsActive=1
                  GROUP BY c.CourseID, c.CourseCode, c.CourseName, c.CreditHours,
                           d.DeptName, i.FullName, c.Semester, c.Capacity, c.IsActive
                  ORDER BY c.CourseName";

            public const string Insert =
                @"INSERT INTO Courses (CourseCode, CourseName, CreditHours, DeptID, InstructorID, Semester, Capacity)
                  VALUES (@CourseCode, @CourseName, @CreditHours, @DeptID, @InstructorID, @Semester, @Capacity)";

            public const string Update =
                @"UPDATE Courses SET
                    CourseName=@CourseName, CreditHours=@CreditHours,
                    DeptID=@DeptID, InstructorID=@InstructorID,
                    Semester=@Semester, Capacity=@Capacity
                  WHERE CourseID=@CourseID";

            public const string Deactivate =
                @"UPDATE Courses SET IsActive=0 WHERE CourseID=@CourseID";

            public const string GetForDropdown =
                @"SELECT CourseID, CourseCode + ' - ' + CourseName AS Display
                  FROM Courses WHERE IsActive=1 ORDER BY CourseName";
        }

        public static class Enrollments
        {
            // Uses stored procedure sp_EnrollStudent
            public const string EnrollSP = "sp_EnrollStudent";

            public const string GetByStudent =
                @"SELECT e.EnrollmentID, c.CourseCode, c.CourseName,
                         c.CreditHours, i.FullName AS Instructor,
                         e.EnrollDate, e.Status
                  FROM Enrollments e
                  JOIN Courses c ON e.CourseID = c.CourseID
                  LEFT JOIN Instructors i ON c.InstructorID = i.InstructorID
                  WHERE e.StudentID = @StudentID AND e.Status='Active'
                  ORDER BY c.CourseName";

            public const string GetAll =
                @"SELECT e.EnrollmentID, s.StudentCode, s.FullName AS StudentName,
                         c.CourseCode, c.CourseName, e.EnrollDate, e.Status
                  FROM Enrollments e
                  JOIN Students s ON e.StudentID = s.StudentID
                  JOIN Courses c ON e.CourseID = c.CourseID
                  ORDER BY e.EnrollDate DESC";

            public const string DropCourse =
                @"UPDATE Enrollments SET Status='Dropped'
                  WHERE EnrollmentID=@EnrollmentID";

            public const string GetAvailableCourses =
                @"SELECT c.CourseID, c.CourseCode + ' - ' + c.CourseName AS Display
                  FROM Courses c
                  WHERE c.IsActive=1
                    AND c.CourseID NOT IN (
                        SELECT CourseID FROM Enrollments
                        WHERE StudentID=@StudentID AND Status='Active')
                  ORDER BY c.CourseName";
        }

        public static class Attendance
        {
            public const string GetByCourse =
                @"SELECT a.AttendanceID, s.StudentCode, s.FullName AS StudentName,
                         a.AttendDate, a.Status, a.Remarks, e.EnrollmentID
                  FROM Attendance a
                  JOIN Enrollments e ON a.EnrollmentID = e.EnrollmentID
                  JOIN Students s ON e.StudentID = s.StudentID
                  WHERE e.CourseID = @CourseID
                  ORDER BY a.AttendDate DESC, s.FullName";

            public const string GetEnrolledForCourse =
                @"SELECT e.EnrollmentID, s.StudentCode, s.FullName AS StudentName
                  FROM Enrollments e
                  JOIN Students s ON e.StudentID = s.StudentID
                  WHERE e.CourseID=@CourseID AND e.Status='Active'
                  ORDER BY s.FullName";

            public const string Insert =
                @"INSERT INTO Attendance (EnrollmentID, AttendDate, Status, MarkedBy, Remarks)
                  VALUES (@EnrollmentID, @Date, @Status, @MarkedBy, @Remarks)";

            public const string Update =
                @"UPDATE Attendance SET Status=@Status, Remarks=@Remarks
                  WHERE AttendanceID=@AttendanceID";

            public const string GetSummary =
                @"SELECT s.StudentCode, s.FullName AS StudentName, c.CourseName,
                         COUNT(*) AS TotalClasses,
                         SUM(CASE WHEN a.Status IN ('Present','Late') THEN 1 ELSE 0 END) AS Attended,
                         CAST(SUM(CASE WHEN a.Status IN ('Present','Late') THEN 1 ELSE 0 END)*100.0
                              /NULLIF(COUNT(*),0) AS DECIMAL(5,2)) AS Percentage
                  FROM Attendance a
                  JOIN Enrollments e ON a.EnrollmentID=e.EnrollmentID
                  JOIN Students s ON e.StudentID=s.StudentID
                  JOIN Courses c ON e.CourseID=c.CourseID
                  GROUP BY s.StudentCode, s.FullName, c.CourseName
                  ORDER BY Percentage";
        }

        public static class Results
        {
            public const string GetAll =
                @"SELECT r.ResultID, s.StudentCode, s.FullName AS StudentName,
                         c.CourseCode, c.CourseName,
                         r.Assignments, r.Midterm, r.FinalExam,
                         r.TotalMarks, r.Grade, r.GradePoints, r.IsLocked
                  FROM Results r
                  JOIN Enrollments e ON r.EnrollmentID=e.EnrollmentID
                  JOIN Students s ON e.StudentID=s.StudentID
                  JOIN Courses c ON e.CourseID=c.CourseID
                  ORDER BY s.FullName";

            public const string GetByStudent =
                @"SELECT r.ResultID, c.CourseCode, c.CourseName, c.CreditHours,
                         r.Assignments, r.Midterm, r.FinalExam,
                         r.TotalMarks, r.Grade, r.GradePoints, r.IsLocked
                  FROM Results r
                  JOIN Enrollments e ON r.EnrollmentID=e.EnrollmentID
                  JOIN Courses c ON e.CourseID=c.CourseID
                  WHERE e.StudentID=@StudentID";

            public const string Insert =
                @"INSERT INTO Results (EnrollmentID, Assignments, Midterm, FinalExam, EnteredBy)
                  VALUES (@EnrollmentID, @Assignments, @Midterm, @FinalExam, @EnteredBy)";

            public const string Update =
                @"UPDATE Results SET
                    Assignments=@Assignments, Midterm=@Midterm, FinalExam=@FinalExam
                  WHERE ResultID=@ResultID AND IsLocked=0";

            public const string LockResult =
                @"UPDATE Results SET IsLocked=1 WHERE ResultID=@ResultID";

            // Runs stored procedure to assign grade after insert/update
            public const string AssignGradeSP = "sp_AssignGrade";

            public const string GetEnrollmentsWithoutResult =
                @"SELECT e.EnrollmentID,
                         s.StudentCode + ' - ' + s.FullName AS StudentDisplay,
                         c.CourseCode + ' - ' + c.CourseName AS CourseDisplay
                  FROM Enrollments e
                  JOIN Students s ON e.StudentID=s.StudentID
                  JOIN Courses c ON e.CourseID=c.CourseID
                  WHERE e.Status='Active'
                    AND e.EnrollmentID NOT IN (SELECT EnrollmentID FROM Results)";
        }

        public static class Departments
        {
            public const string GetAll =
                @"SELECT DeptID, DeptName, DeptCode, Description, CreatedAt
                  FROM Departments ORDER BY DeptName";

            public const string GetForDropdown =
                @"SELECT DeptID, DeptName FROM Departments ORDER BY DeptName";

            public const string Insert =
                @"INSERT INTO Departments (DeptName, DeptCode, Description)
                  VALUES (@DeptName, @DeptCode, @Description)";

            public const string Update =
                @"UPDATE Departments SET DeptName=@DeptName, DeptCode=@DeptCode,
                    Description=@Description WHERE DeptID=@DeptID";

            public const string Delete =
                @"DELETE FROM Departments WHERE DeptID=@DeptID";
        }

        public static class Dashboard
        {
            public const string GetStats =
                @"SELECT
                    (SELECT COUNT(*) FROM Students WHERE IsActive=1)       AS TotalStudents,
                    (SELECT COUNT(*) FROM Instructors WHERE IsActive=1)    AS TotalInstructors,
                    (SELECT COUNT(*) FROM Courses WHERE IsActive=1)        AS TotalCourses,
                    (SELECT COUNT(*) FROM Enrollments WHERE Status='Active') AS TotalEnrollments,
                    (SELECT ROUND(AVG(CAST(CGPA AS FLOAT)),2) FROM Students WHERE IsActive=1) AS AvgCGPA";

            public const string GetTopStudents =
                @"SELECT TOP 5 s.StudentCode, s.FullName, d.DeptName, s.CGPA
                  FROM Students s
                  LEFT JOIN Departments d ON s.DeptID=d.DeptID
                  WHERE s.IsActive=1 AND s.CGPA > 0
                  ORDER BY s.CGPA DESC";

            public const string GetRecentEnrollments =
                @"SELECT TOP 8 s.FullName AS Student, c.CourseName, e.EnrollDate
                  FROM Enrollments e
                  JOIN Students s ON e.StudentID=s.StudentID
                  JOIN Courses c ON e.CourseID=c.CourseID
                  ORDER BY e.EnrollDate DESC";

            public const string GetDeptEnrollmentStats =
                @"SELECT d.DeptName, COUNT(s.StudentID) AS StudentCount
                  FROM Departments d
                  LEFT JOIN Students s ON d.DeptID=s.DeptID AND s.IsActive=1
                  GROUP BY d.DeptName
                  ORDER BY StudentCount DESC";
        }

        public static class Notifications
        {
            public const string GetUnread =
                @"SELECT NotifID, Title, Message, CreatedAt
                  FROM Notifications WHERE IsRead=0 ORDER BY CreatedAt DESC";

            public const string MarkRead =
                @"UPDATE Notifications SET IsRead=1 WHERE NotifID=@NotifID";

            public const string Insert =
                @"INSERT INTO Notifications (UserID, Title, Message)
                  VALUES (@UserID, @Title, @Message)";
        }

        public static class AuditLog
        {
            public const string GetAll =
                @"SELECT TOP 100 LogID, TableName, Action, RecordID,
                         ChangedBy, ChangeTime, Details
                  FROM AuditLog ORDER BY ChangeTime DESC";
        }
    }
}