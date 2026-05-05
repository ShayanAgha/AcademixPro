USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AcademixProDB')
    CREATE DATABASE AcademixProDB;
GO

USE AcademixProDB;
GO

-- ============================================================
--  DROP TABLES (reverse FK order)
-- ============================================================
IF OBJECT_ID('Notifications', 'U') IS NOT NULL DROP TABLE Notifications;
IF OBJECT_ID('AuditLog',      'U') IS NOT NULL DROP TABLE AuditLog;
IF OBJECT_ID('Results',       'U') IS NOT NULL DROP TABLE Results;
IF OBJECT_ID('Attendance',    'U') IS NOT NULL DROP TABLE Attendance;
IF OBJECT_ID('Enrollments',   'U') IS NOT NULL DROP TABLE Enrollments;
IF OBJECT_ID('Courses',       'U') IS NOT NULL DROP TABLE Courses;
IF OBJECT_ID('Students',      'U') IS NOT NULL DROP TABLE Students;
IF OBJECT_ID('Instructors',   'U') IS NOT NULL DROP TABLE Instructors;
IF OBJECT_ID('Departments',   'U') IS NOT NULL DROP TABLE Departments;
IF OBJECT_ID('Users',         'U') IS NOT NULL DROP TABLE Users;
GO

-- ============================================================
--  TABLE DEFINITIONS
-- ============================================================

CREATE TABLE Departments (
    DeptID      INT IDENTITY(1,1) PRIMARY KEY,
    DeptName    NVARCHAR(100) NOT NULL UNIQUE,
    DeptCode    NVARCHAR(10)  NOT NULL UNIQUE,
    Description NVARCHAR(255),
    CreatedAt   DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Users (
    UserID       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(150) NOT NULL UNIQUE,
    Role         NVARCHAR(20)  NOT NULL CHECK (Role IN ('Admin','Teacher','Student')),
    IsActive     BIT DEFAULT 1,
    LastLogin    DATETIME,
    CreatedAt    DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Students (
    StudentID     INT IDENTITY(1,1) PRIMARY KEY,
    StudentCode   NVARCHAR(20)  NOT NULL UNIQUE,
    FullName      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(150) NOT NULL UNIQUE,
    Phone         NVARCHAR(20),
    DateOfBirth   DATE,
    Gender        NVARCHAR(10)  CHECK (Gender IN ('Male','Female','Other')),
    Address       NVARCHAR(255),
    DeptID        INT REFERENCES Departments(DeptID),
    AdmissionDate DATE DEFAULT GETDATE(),
    Semester      INT  CHECK (Semester BETWEEN 1 AND 12),
    CGPA          DECIMAL(4,2) DEFAULT 0.00,
    IsActive      BIT DEFAULT 1,
    CreatedAt     DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Instructors (
    InstructorID   INT IDENTITY(1,1) PRIMARY KEY,
    InstructorCode NVARCHAR(20)  NOT NULL UNIQUE,
    FullName       NVARCHAR(100) NOT NULL,
    Email          NVARCHAR(150) NOT NULL UNIQUE,
    Phone          NVARCHAR(20),
    Specialization NVARCHAR(100),
    DeptID         INT REFERENCES Departments(DeptID),
    JoiningDate    DATE DEFAULT GETDATE(),
    IsActive       BIT DEFAULT 1,
    CreatedAt      DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Courses (
    CourseID     INT IDENTITY(1,1) PRIMARY KEY,
    CourseCode   NVARCHAR(20)  NOT NULL UNIQUE,
    CourseName   NVARCHAR(150) NOT NULL,
    CreditHours  INT NOT NULL CHECK (CreditHours BETWEEN 1 AND 6),
    DeptID       INT REFERENCES Departments(DeptID),
    InstructorID INT REFERENCES Instructors(InstructorID),
    Semester     INT CHECK (Semester BETWEEN 1 AND 12),
    Capacity     INT DEFAULT 40,
    IsActive     BIT DEFAULT 1,
    CreatedAt    DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Enrollments (
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID    INT NOT NULL REFERENCES Students(StudentID),
    CourseID     INT NOT NULL REFERENCES Courses(CourseID),
    EnrollDate   DATE DEFAULT GETDATE(),
    Status       NVARCHAR(20) DEFAULT 'Active'
                 CHECK (Status IN ('Active','Dropped','Completed')),
    CONSTRAINT UQ_Enrollment UNIQUE (StudentID, CourseID)
);
GO

CREATE TABLE Attendance (
    AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL REFERENCES Enrollments(EnrollmentID),
    AttendDate   DATE NOT NULL DEFAULT GETDATE(),
    Status       NVARCHAR(10) NOT NULL CHECK (Status IN ('Present','Absent','Late')),
    MarkedBy     INT REFERENCES Users(UserID),
    Remarks      NVARCHAR(200),
    CONSTRAINT UQ_Attendance UNIQUE (EnrollmentID, AttendDate)
);
GO

CREATE TABLE Results (
    ResultID     INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID INT NOT NULL REFERENCES Enrollments(EnrollmentID) UNIQUE,
    Assignments  DECIMAL(5,2) DEFAULT 0 CHECK (Assignments BETWEEN 0 AND 20),
    Midterm      DECIMAL(5,2) DEFAULT 0 CHECK (Midterm     BETWEEN 0 AND 30),
    FinalExam    DECIMAL(5,2) DEFAULT 0 CHECK (FinalExam   BETWEEN 0 AND 50),
    TotalMarks   AS (Assignments + Midterm + FinalExam),
    Grade        NVARCHAR(5),
    GradePoints  DECIMAL(3,2),
    IsLocked     BIT DEFAULT 0,
    EnteredBy    INT REFERENCES Users(UserID),
    EnteredAt    DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE Notifications (
    NotifID   INT IDENTITY(1,1) PRIMARY KEY,
    UserID    INT REFERENCES Users(UserID),
    Title     NVARCHAR(100) NOT NULL,
    Message   NVARCHAR(500) NOT NULL,
    IsRead    BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE AuditLog (
    LogID      INT IDENTITY(1,1) PRIMARY KEY,
    TableName  NVARCHAR(50)  NOT NULL,
    Action     NVARCHAR(10)  NOT NULL,
    RecordID   INT,
    ChangedBy  NVARCHAR(100),
    ChangeTime DATETIME DEFAULT GETDATE(),
    Details    NVARCHAR(500)
);
GO

-- ============================================================
--  STORED PROCEDURES
-- ============================================================

CREATE OR ALTER PROCEDURE sp_EnrollStudent
    @StudentID INT,
    @CourseID  INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF EXISTS (SELECT 1 FROM Enrollments WHERE StudentID=@StudentID AND CourseID=@CourseID)
        BEGIN RAISERROR('Student is already enrolled in this course.',16,1); ROLLBACK; RETURN; END

        DECLARE @Capacity INT, @Enrolled INT;
        SELECT @Capacity = Capacity FROM Courses WHERE CourseID = @CourseID;
        SELECT @Enrolled = COUNT(*) FROM Enrollments WHERE CourseID=@CourseID AND Status='Active';
        IF @Enrolled >= @Capacity
        BEGIN RAISERROR('Course has reached maximum capacity.',16,1); ROLLBACK; RETURN; END

        INSERT INTO Enrollments (StudentID, CourseID) VALUES (@StudentID, @CourseID);
        COMMIT;
        SELECT 'Enrollment successful.' AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK; THROW;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE sp_AssignGrade
    @ResultID INT
AS
BEGIN
    DECLARE @Total DECIMAL(5,2);
    SELECT @Total = TotalMarks FROM Results WHERE ResultID = @ResultID;

    DECLARE @Grade NVARCHAR(5), @GPA DECIMAL(3,2);
    SELECT
        @Grade = CASE
            WHEN @Total >= 90 THEN 'A+'  WHEN @Total >= 85 THEN 'A'
            WHEN @Total >= 80 THEN 'A-'  WHEN @Total >= 75 THEN 'B+'
            WHEN @Total >= 70 THEN 'B'   WHEN @Total >= 65 THEN 'B-'
            WHEN @Total >= 60 THEN 'C+'  WHEN @Total >= 55 THEN 'C'
            WHEN @Total >= 50 THEN 'D'   ELSE 'F'
        END,
        @GPA = CASE
            WHEN @Total >= 90 THEN 4.0  WHEN @Total >= 85 THEN 4.0
            WHEN @Total >= 80 THEN 3.7  WHEN @Total >= 75 THEN 3.3
            WHEN @Total >= 70 THEN 3.0  WHEN @Total >= 65 THEN 2.7
            WHEN @Total >= 60 THEN 2.3  WHEN @Total >= 55 THEN 2.0
            WHEN @Total >= 50 THEN 1.0  ELSE 0.0
        END;

    UPDATE Results SET Grade = @Grade, GradePoints = @GPA WHERE ResultID = @ResultID;

    DECLARE @StudentID INT;
    SELECT @StudentID = e.StudentID
    FROM Results r JOIN Enrollments e ON r.EnrollmentID = e.EnrollmentID
    WHERE r.ResultID = @ResultID;

    UPDATE Students SET CGPA = (
        SELECT ROUND(AVG(r2.GradePoints), 2)
        FROM Results r2 JOIN Enrollments e2 ON r2.EnrollmentID = e2.EnrollmentID
        WHERE e2.StudentID = @StudentID AND r2.GradePoints IS NOT NULL
    ) WHERE StudentID = @StudentID;
END;
GO

CREATE OR ALTER PROCEDURE sp_GetAttendancePercentage
    @EnrollmentID INT
AS
BEGIN
    SELECT
        COUNT(*) AS TotalClasses,
        SUM(CASE WHEN Status IN ('Present','Late') THEN 1 ELSE 0 END) AS Attended,
        CAST(
            SUM(CASE WHEN Status IN ('Present','Late') THEN 1 ELSE 0 END) * 100.0
            / NULLIF(COUNT(*), 0) AS DECIMAL(5,2)
        ) AS AttendancePercent
    FROM Attendance WHERE EnrollmentID = @EnrollmentID;
END;
GO

CREATE OR ALTER PROCEDURE sp_AuthenticateUser
    @Username     NVARCHAR(50),
    @PasswordHash NVARCHAR(256)
AS
BEGIN
    SELECT UserID, Username, FullName, Email, Role, IsActive
    FROM Users
    WHERE Username=@Username AND PasswordHash=@PasswordHash AND IsActive=1;

    UPDATE Users SET LastLogin=GETDATE()
    WHERE Username=@Username AND PasswordHash=@PasswordHash;
END;
GO

-- ============================================================
--  TRIGGERS
-- ============================================================

CREATE OR ALTER TRIGGER trg_Students_Audit
ON Students AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS(SELECT 1 FROM inserted)
        INSERT INTO AuditLog(TableName, Action, RecordID, Details)
        SELECT 'Students',
               CASE WHEN EXISTS(SELECT 1 FROM deleted) THEN 'UPDATE' ELSE 'INSERT' END,
               StudentID, 'Student record modified: ' + FullName
        FROM inserted;
    ELSE
        INSERT INTO AuditLog(TableName, Action, RecordID, Details)
        SELECT 'Students','DELETE',StudentID,'Student deleted: '+FullName FROM deleted;
END;
GO

CREATE OR ALTER TRIGGER trg_LowAttendance
ON Attendance AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EnrollmentID INT, @Pct DECIMAL(5,2);
    SELECT @EnrollmentID = EnrollmentID FROM inserted;

    SELECT @Pct = CAST(
        SUM(CASE WHEN Status IN ('Present','Late') THEN 1 ELSE 0 END) * 100.0
        / NULLIF(COUNT(*),0) AS DECIMAL(5,2))
    FROM Attendance WHERE EnrollmentID = @EnrollmentID;

    IF @Pct < 75
    BEGIN
        DECLARE @StudentID INT;
        SELECT @StudentID = e.StudentID FROM Enrollments e WHERE e.EnrollmentID=@EnrollmentID;
        IF NOT EXISTS (
            SELECT 1 FROM Notifications
            WHERE UserID=@StudentID AND Title='Low Attendance Warning'
              AND CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)
        )
        INSERT INTO Notifications(UserID, Title, Message)
        SELECT s.StudentID,
               'Low Attendance Warning',
               'Your attendance has dropped to '+CAST(@Pct AS NVARCHAR)+'%. Minimum required is 75%.'
        FROM Students s WHERE s.StudentID=@StudentID;
    END
END;
GO

-- ============================================================
--  VIEWS
-- ============================================================

CREATE OR ALTER VIEW vw_StudentDashboard AS
SELECT s.StudentID, s.StudentCode, s.FullName AS StudentName,
       s.Email, s.Semester, s.CGPA, d.DeptName AS Department,
       COUNT(e.EnrollmentID) AS CoursesEnrolled
FROM Students s
LEFT JOIN Departments d ON s.DeptID = d.DeptID
LEFT JOIN Enrollments e ON s.StudentID = e.StudentID AND e.Status = 'Active'
WHERE s.IsActive = 1
GROUP BY s.StudentID, s.StudentCode, s.FullName, s.Email, s.Semester, s.CGPA, d.DeptName;
GO

CREATE OR ALTER VIEW vw_CourseEnrollmentStatus AS
SELECT c.CourseID, c.CourseCode, c.CourseName, c.CreditHours, c.Capacity,
       COUNT(e.EnrollmentID) AS EnrolledCount,
       c.Capacity - COUNT(e.EnrollmentID) AS SeatsAvailable,
       i.FullName AS InstructorName, d.DeptName
FROM Courses c
LEFT JOIN Enrollments e ON c.CourseID = e.CourseID AND e.Status = 'Active'
LEFT JOIN Instructors i ON c.InstructorID = i.InstructorID
LEFT JOIN Departments d ON c.DeptID = d.DeptID
WHERE c.IsActive = 1
GROUP BY c.CourseID, c.CourseCode, c.CourseName, c.CreditHours, c.Capacity, i.FullName, d.DeptName;
GO

CREATE OR ALTER VIEW vw_ResultSheet AS
SELECT s.StudentCode, s.FullName AS StudentName,
       c.CourseCode, c.CourseName,
       r.Assignments, r.Midterm, r.FinalExam,
       r.TotalMarks, r.Grade, r.GradePoints, r.IsLocked, d.DeptName
FROM Results r
JOIN Enrollments e ON r.EnrollmentID = e.EnrollmentID
JOIN Students s    ON e.StudentID    = s.StudentID
JOIN Courses c     ON e.CourseID     = c.CourseID
JOIN Departments d ON s.DeptID       = d.DeptID;
GO

CREATE OR ALTER VIEW vw_AttendanceSummary AS
SELECT s.StudentCode, s.FullName AS StudentName, c.CourseName,
       COUNT(*) AS TotalClasses,
       SUM(CASE WHEN a.Status IN ('Present','Late') THEN 1 ELSE 0 END) AS Attended,
       CAST(SUM(CASE WHEN a.Status IN ('Present','Late') THEN 1 ELSE 0 END)*100.0
            / NULLIF(COUNT(*),0) AS DECIMAL(5,2)) AS AttendancePercent
FROM Attendance a
JOIN Enrollments e ON a.EnrollmentID = e.EnrollmentID
JOIN Students s    ON e.StudentID    = s.StudentID
JOIN Courses c     ON e.CourseID     = c.CourseID
GROUP BY s.StudentCode, s.FullName, c.CourseName;
GO

-- ============================================================
--  SAMPLE DATA
-- ============================================================

-- ---- 1. DEPARTMENTS -----------------------------------------
-- DeptID: 1=CS  2=EE  3=BA  4=MATH  5=SE
INSERT INTO Departments (DeptName, DeptCode, Description) VALUES
('Computer Science',        'CS',   'Software, AI and systems'),
('Electrical Engineering',  'EE',   'Circuits and electronics'),
('Business Administration', 'BA',   'Management and commerce'),
('Mathematics',             'MATH', 'Pure and applied mathematics'),
('Software Engineering',    'SE',   'Software design, development and quality assurance');
GO

-- ---- 2. USERS -----------------------------------------------
INSERT INTO Users (Username, PasswordHash, FullName, Email, Role) VALUES
('admin',     'E86F78A8A3CAF0B60D8E74E5942AA6D86DC150CD3C03338AEF25B7D2D7E3ACC7', 'System Administrator', 'admin@academix.edu',     'Admin'),
('registrar', 'A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0C1D2E3F4A5B6C7D8E9F0A1B2', 'Registrar Office',     'registrar@academix.edu', 'Admin'),
('teacher1',  'BF3EF2AB1DC54C0A9E7B2F6C3A85D1E9F0B4C7A2D6E8F1B3C5A7D9E0F2B4C6A8', 'Dr. Sarah Ahmed',      'sarah@academix.edu',     'Teacher'),
('teacher2',  'CC4DE3BC2ED65D1BAE8C3F7D4B96E2FAF1C5D8B3E7F2C4B6D9E1F5C8B0D3E7B0', 'Prof. Imran Khan',     'imran@academix.edu',     'Teacher'),
('teacher3',  'DD5EF4CD3FE76E2CBF9D4G8E5C07F3GBG2D6E9C4F8G3D5C7E0F2G6D9C1E4F8C1', 'Ms. Fatima Malik',     'fatima@academix.edu',    'Teacher'),
('teacher4',  'EE6FG5DE4GF87F3DCG0E5H9F6D18G4HCH3E7F0D5G9H4E6D8F1G3H7E0D2F5G9D2', 'Dr. Zubair Shah',      'zubair@academix.edu',    'Teacher'),
('teacher5',  'FF7GH6EF5HG98G4EDH1F6I0G7E29H5IJI4F8G1E6H0I5F7E9G2H4I8F1E3G6H0E3', 'Dr. Nadia Hussain',    'nadia@academix.edu',     'Teacher'),
('teacher6',  'GG8HI7FG6IH09H5FEI2G7J1H8F30I6JJJ5G9H2F7I1J6G8F0H3I5J9G2F4H7I1F4', 'Mr. Kamran Baig',      'kamran@academix.edu',    'Teacher'),
('teacher7',  'HH9IJ8GH7JI10I6GFJ3H8K2I9G41J7KKK6H0I3G8J2K7H9G1I4J6K0H3G5I8J2G5', 'Dr. Hassan Mirza',     'hassan@academix.edu',    'Teacher'),
('teacher8',  'II0JK9HI8KJ21J7HGK4I9L3J0H52K8LLL7I1J4H9K3L8I0J5K7L1I4H6J9K3H6',  'Ms. Amna Qureshi',     'amna@academix.edu',      'Teacher');
GO

INSERT INTO Users (Username, PasswordHash, FullName, Email, Role) VALUES
('student1',  'CF2D1AB0EC43B9F8D6A1E5B2C7F4A3E8D1B6C9A0E3F7B2D5A8E1F4B7C0A3E6B9', 'Ali Hassan',       'ali@academix.edu',    'Student'),
('student2',  'DG3E2BC1FD54C0G9E7B2F6C3A95D1E9F0B4C7A2D6E8F1B3C5A7D9E0F2B4C6A9',  'Ayesha Siddiqui',  'ayesha@academix.edu', 'Student'),
('student3',  'EH4F3CD2GE65D1H0F8C3G7D4B06E2FAG1C5D8B3E7G2C4B6D9E1G5C8B0D3E7C0',  'Usman Ghani',      'usman@academix.edu',  'Student'),
('student4',  'FI5G4DE3HF76E2I1G9D4H8E5C17F3GBH2D6E9C4F8H3D5C7E0F2H6D9C1E4F8D1',  'Hamza Rehman',     'hamza@academix.edu',  'Student'),
('student5',  'GJ6H5EF4IG87F3J2H0E5I9F6D28G4HCI3E7F0D5G9I4E6D8F1G3I7E0D2F5G9E2',  'Farhan Iqbal',     'farhan@academix.edu', 'Student'),
('student6',  'HK7I6FG5JH98G4K3I1F6J0G7E39H5IJJ4F8G1E6H0J5F7E9G2H4J8F1E3G6H0F3',  'Bilal Ahmed',      'bilal@academix.edu',  'Student'),
('student7',  'IL8J7GH6KI09H5L4J2G7K1H8F40I6JKK5G9H2F7I1K6G8F0H3I5K9G2F4H7I1G4',  'Mariam Noor',      'mariam@academix.edu', 'Student'),
('student8',  'JM9K8HI7LJ10I6M5K3H8L2I9G51J7KLL6H0I3G8J2L7H9G1I4J6L0H3G5I8J2H5',  'Sana Tariq',       'sana@academix.edu',   'Student'),
('student9',  'KN0L9IJ8MK21J7N6L4I9M3J0H62K8LMM7I1J4H9K3M8I0J5K7M1I4H6J9K3I6',   'Zara Khan',        'zara@academix.edu',   'Student'),
('student10', 'LO1M0JK9NL32K8O7M5J0N4K1I73L9MNN8J2K5I0L4N9J1K6L8N2J5I7K0L4J7',   'Hina Baig',        'hina@academix.edu',   'Student'),
('student11', 'MP2N1KL0OM43L9P8N6K1O5L2J84M0NOO9K3L6J1M5O0K2L7M9O3K6J8L1M5K8',   'Tariq Mahmood',    'tariq@academix.edu',  'Student'),
('student12', 'NQ3O2LM1PN54M0Q9O7L2P6M3K95N1OPP0L4M7K2N6P1L3M8N0P4L7K9M2N6L9',   'Rida Fatima',      'rida@academix.edu',   'Student'),
('student13', 'OR4P3MN2QO65N1R0P8M3Q7N4L06O2PQQ1M5N8L3O7Q2M4N9O1Q5M8L0N3O7M0',   'Asad Mehmood',     'asad@academix.edu',   'Student'),
('student14', 'PS5Q4NO3RP76O2S1Q9N4R8O5M17P3QRR2N6O9M4P8R3N5O0P2R6N9M1O4P8N1',   'Noor Ul Ain',      'noor@academix.edu',   'Student'),
('student15', 'QT6R5OP4SQ87P3T2R0O5S9P6N28Q4RSS3O7P0N5Q9S4O6P1Q3S7O0N2P5Q9O2',   'Saad Abdullah',    'saad@academix.edu',   'Student');
GO

-- ---- 3. INSTRUCTORS -----------------------------------------
-- InstructorID: 1=Sarah  2=Imran  3=Fatima  4=Zubair
--               5=Nadia  6=Kamran 7=Hassan  8=Amna
INSERT INTO Instructors (InstructorCode, FullName, Email, Phone, Specialization, DeptID, JoiningDate) VALUES
('INS-001', 'Dr. Sarah Ahmed',   'sarah@academix.edu',   '0300-1111111', 'Artificial Intelligence',      1, '2018-08-01'),
('INS-002', 'Prof. Imran Khan',  'imran@academix.edu',   '0300-2222222', 'Digital Systems',              2, '2015-03-15'),
('INS-003', 'Ms. Fatima Malik',  'fatima@academix.edu',  '0300-3333333', 'Marketing & Strategy',         3, '2019-01-10'),
('INS-004', 'Dr. Zubair Shah',   'zubair@academix.edu',  '0300-4444444', 'Calculus & Linear Algebra',    4, '2016-07-20'),
('INS-005', 'Dr. Nadia Hussain', 'nadia@academix.edu',   '0300-5555555', 'Web & Cloud Technologies',     5, '2020-02-01'),
('INS-006', 'Mr. Kamran Baig',   'kamran@academix.edu',  '0300-6666666', 'Software Testing & DevOps',    5, '2021-08-15'),
('INS-007', 'Dr. Hassan Mirza',  'hassan@academix.edu',  '0300-7777777', 'Operating Systems & Networks', 1, '2017-09-01'),
('INS-008', 'Ms. Amna Qureshi',  'amna@academix.edu',    '0300-8888888', 'Financial Accounting',         3, '2022-01-05');
GO

-- ---- 4. COURSES ---------------------------------------------
-- CourseID: 1=CS-101  2=CS-201  3=CS-301  4=CS-401  5=CS-501
--           6=EE-101  7=EE-201
--           8=BA-101  9=BA-201  10=BA-301
--           11=MATH-101  12=MATH-201
--           13=SE-101  14=SE-201  15=SE-301  16=SE-401
INSERT INTO Courses (CourseCode, CourseName, CreditHours, DeptID, InstructorID, Semester, Capacity) VALUES
('CS-101',   'Introduction to Programming',       3, 1, 1, 1, 40),
('CS-201',   'Data Structures',                   3, 1, 1, 2, 35),
('CS-301',   'Database Systems',                  3, 1, 7, 3, 30),
('CS-401',   'Artificial Intelligence',           3, 1, 1, 4, 30),
('CS-501',   'Operating Systems',                 3, 1, 7, 5, 35),
('EE-101',   'Circuit Analysis',                  3, 2, 2, 1, 35),
('EE-201',   'Digital Logic Design',              3, 2, 2, 2, 30),
('BA-101',   'Principles of Management',          3, 3, 3, 1, 50),
('BA-201',   'Financial Accounting',              3, 3, 8, 2, 45),
('BA-301',   'Business Statistics',               3, 3, 3, 3, 40),
('MATH-101', 'Calculus I',                        3, 4, 4, 1, 45),
('MATH-201', 'Linear Algebra',                    3, 4, 4, 2, 40),
('SE-101',   'Software Requirements Engineering', 3, 5, 5, 1, 40),
('SE-201',   'Software Design & Architecture',    3, 5, 5, 2, 35),
('SE-301',   'Software Testing & QA',             3, 5, 6, 3, 35),
('SE-401',   'DevOps & CI/CD Pipelines',          3, 5, 6, 4, 30);
GO

-- ---- 5. STUDENTS --------------------------------------------
-- StudentID: 1=Ali    2=Ayesha  3=Usman   4=Hamza   5=Farhan
--            6=Bilal  7=Mariam
--            8=Sana   9=Zara    10=Hina
--            11=Tariq 12=Rida   13=Asad   14=Noor   15=Saad
INSERT INTO Students (StudentCode, FullName, Email, Phone, DateOfBirth, Gender, Address, DeptID, AdmissionDate, Semester) VALUES
('STU-2024-001', 'Ali Hassan',       'ali@academix.edu',      '0311-1111111', '2002-03-15', 'Male',   'Karachi',    1, '2024-01-15', 1),
('STU-2024-002', 'Ayesha Siddiqui', 'ayesha@academix.edu',   '0311-2222222', '2002-07-22', 'Female', 'Lahore',     1, '2024-01-15', 1),
('STU-2023-001', 'Usman Ghani',      'usman@academix.edu',    '0311-5555555', '2001-06-18', 'Male',   'Quetta',     1, '2023-08-20', 3),
('STU-2023-002', 'Hamza Rehman',     'hamza@academix.edu',    '0311-7777777', '2001-09-10', 'Male',   'Faisalabad', 1, '2023-08-20', 3),
('STU-2022-001', 'Farhan Iqbal',     'farhan@academix.edu',   '0311-9999999', '2000-04-25', 'Male',   'Multan',     1, '2022-08-20', 5),
('STU-2023-003', 'Bilal Ahmed',      'bilal@academix.edu',    '0311-3333333', '2001-11-05', 'Male',   'Islamabad',  2, '2023-08-20', 2),
('STU-2024-003', 'Mariam Noor',      'mariam@academix.edu',   '0311-6666666', '2003-02-14', 'Female', 'Rawalpindi', 2, '2024-01-15', 1),
('STU-2024-004', 'Sana Tariq',       'sana@academix.edu',     '0311-4444444', '2003-01-30', 'Female', 'Peshawar',   3, '2024-01-15', 1),
('STU-2023-004', 'Zara Khan',        'zara@academix.edu',     '0311-8888888', '2002-12-03', 'Female', 'Hyderabad',  3, '2023-08-20', 2),
('STU-2022-002', 'Hina Baig',        'hina@academix.edu',     '0311-1010101', '2000-08-17', 'Female', 'Sialkot',    3, '2022-01-15', 4),
('STU-2024-005', 'Tariq Mahmood',    'tariq@academix.edu',    '0311-1111222', '2003-05-08', 'Male',   'Lahore',     5, '2024-01-15', 1),
('STU-2024-006', 'Rida Fatima',      'rida@academix.edu',     '0311-2222333', '2003-09-19', 'Female', 'Karachi',    5, '2024-01-15', 1),
('STU-2023-005', 'Asad Mehmood',     'asad@academix.edu',     '0311-3333444', '2001-03-27', 'Male',   'Lahore',     5, '2023-08-20', 2),
('STU-2024-007', 'Noor Ul Ain',      'noor@academix.edu',     '0311-4444555', '2002-11-11', 'Female', 'Islamabad',  5, '2024-01-15', 1),
('STU-2023-006', 'Saad Abdullah',    'saad@academix.edu',     '0311-5555666', '2001-07-23', 'Male',   'Peshawar',   5, '2023-08-20', 2);
GO

-- ---- 6. ENROLLMENTS -----------------------------------------
-- One INSERT per row — a single duplicate can NEVER silently
-- abort another student's enrollments.
--
-- Verified EnrollmentID map (StudentID, CourseID — no repeats):
--  E01: S1  C1    Ali Hassan       – CS-101
--  E02: S1  C11   Ali Hassan       – MATH-101
--  E03: S2  C1    Ayesha Siddiqui  – CS-101
--  E04: S2  C11   Ayesha Siddiqui  – MATH-101
--  E05: S3  C2    Usman Ghani      – CS-201
--  E06: S3  C3    Usman Ghani      – CS-301
--  E07: S3  C5    Usman Ghani      – CS-501
--  E08: S4  C2    Hamza Rehman     – CS-201
--  E09: S4  C3    Hamza Rehman     – CS-301
--  E10: S5  C4    Farhan Iqbal     – CS-401
--  E11: S5  C5    Farhan Iqbal     – CS-501
--  E12: S6  C6    Bilal Ahmed      – EE-101
--  E13: S6  C7    Bilal Ahmed      – EE-201
--  E14: S7  C6    Mariam Noor      – EE-101
--  E15: S7  C11   Mariam Noor      – MATH-101
--  E16: S8  C8    Sana Tariq       – BA-101
--  E17: S9  C8    Zara Khan        – BA-101
--  E18: S9  C9    Zara Khan        – BA-201
--  E19: S10 C9    Hina Baig        – BA-201
--  E20: S10 C10   Hina Baig        – BA-301
--  E21: S11 C13   Tariq Mahmood    – SE-101
--  E22: S11 C1    Tariq Mahmood    – CS-101  (cross-dept elective)
--  E23: S12 C13   Rida Fatima      – SE-101
--  E24: S12 C11   Rida Fatima      – MATH-101 (cross-dept elective)
--  E25: S13 C13   Asad Mehmood     – SE-101
--  E26: S13 C14   Asad Mehmood     – SE-201
--  E27: S14 C13   Noor Ul Ain      – SE-101
--  E28: S14 C11   Noor Ul Ain      – MATH-101 (cross-dept elective)
--  E29: S15 C14   Saad Abdullah    – SE-201
--  E30: S15 C15   Saad Abdullah    – SE-301

INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (1,  1,  '2024-01-20'); -- E01
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (1,  11, '2024-01-20'); -- E02
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (2,  1,  '2024-01-20'); -- E03
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (2,  11, '2024-01-20'); -- E04
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (3,  2,  '2023-08-25'); -- E05
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (3,  3,  '2023-08-25'); -- E06
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (3,  5,  '2023-08-25'); -- E07
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (4,  2,  '2023-08-25'); -- E08
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (4,  3,  '2023-08-25'); -- E09
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (5,  4,  '2022-08-25'); -- E10
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (5,  5,  '2022-08-25'); -- E11
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (6,  6,  '2023-08-25'); -- E12
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (6,  7,  '2023-08-25'); -- E13
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (7,  6,  '2024-01-20'); -- E14
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (7,  11, '2024-01-20'); -- E15
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (8,  8,  '2024-01-20'); -- E16
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (9,  8,  '2023-08-25'); -- E17
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (9,  9,  '2023-08-25'); -- E18
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (10, 9,  '2022-01-20'); -- E19
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (10, 10, '2022-01-20'); -- E20
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (11, 13, '2024-01-20'); -- E21
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (11, 1,  '2024-01-20'); -- E22
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (12, 13, '2024-01-20'); -- E23
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (12, 11, '2024-01-20'); -- E24
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (13, 13, '2023-08-25'); -- E25
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (13, 14, '2023-08-25'); -- E26
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (14, 13, '2024-01-20'); -- E27
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (14, 11, '2024-01-20'); -- E28
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (15, 14, '2023-08-25'); -- E29
INSERT INTO Enrollments (StudentID, CourseID, EnrollDate) VALUES (15, 15, '2023-08-25'); -- E30
GO

-- ---- 7. ATTENDANCE ------------------------------------------

-- E01 Ali Hassan – CS-101 (strong: 11 present/late out of 12)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(1,'2024-02-05','Present'),(1,'2024-02-07','Present'),(1,'2024-02-12','Present'),
(1,'2024-02-14','Absent'), (1,'2024-02-19','Present'),(1,'2024-02-21','Present'),
(1,'2024-02-26','Present'),(1,'2024-02-28','Late'),   (1,'2024-03-04','Present'),
(1,'2024-03-06','Present'),(1,'2024-03-11','Present'),(1,'2024-03-13','Present');

-- E02 Ali Hassan – MATH-101 (perfect: 10/10)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(2,'2024-02-05','Present'),(2,'2024-02-07','Present'),(2,'2024-02-12','Present'),
(2,'2024-02-14','Present'),(2,'2024-02-19','Present'),(2,'2024-02-21','Present'),
(2,'2024-02-26','Present'),(2,'2024-02-28','Present'),(2,'2024-03-04','Present'),
(2,'2024-03-06','Late');

-- E03 Ayesha Siddiqui – CS-101 (good: 8/10)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(3,'2024-02-05','Present'),(3,'2024-02-07','Late'),   (3,'2024-02-12','Present'),
(3,'2024-02-14','Present'),(3,'2024-02-19','Absent'), (3,'2024-02-21','Present'),
(3,'2024-02-26','Present'),(3,'2024-02-28','Present'),(3,'2024-03-04','Absent'),
(3,'2024-03-06','Present');

-- E04 Ayesha Siddiqui – MATH-101 (low: 4/8 = 50% ? triggers notification)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(4,'2024-02-05','Absent'), (4,'2024-02-07','Absent'),(4,'2024-02-12','Present'),
(4,'2024-02-14','Present'),(4,'2024-02-19','Absent'),(4,'2024-02-21','Present'),
(4,'2024-02-26','Absent'), (4,'2024-02-28','Present');

-- E05 Usman Ghani – CS-201 (good: 9/10)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(5,'2024-02-05','Present'),(5,'2024-02-07','Present'),(5,'2024-02-12','Absent'),
(5,'2024-02-14','Present'),(5,'2024-02-19','Present'),(5,'2024-02-21','Present'),
(5,'2024-02-26','Late'),   (5,'2024-02-28','Present'),(5,'2024-03-04','Present'),
(5,'2024-03-06','Present');

-- E06 Usman Ghani – CS-301 (excellent: 11/12)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(6,'2024-02-05','Present'),(6,'2024-02-07','Present'),(6,'2024-02-12','Present'),
(6,'2024-02-14','Late'),   (6,'2024-02-19','Present'),(6,'2024-02-21','Present'),
(6,'2024-02-26','Present'),(6,'2024-02-28','Present'),(6,'2024-03-04','Present'),
(6,'2024-03-06','Present'),(6,'2024-03-11','Absent'), (6,'2024-03-13','Present');

-- E08 Hamza Rehman – CS-201 (borderline: 7/12 = 58% ? triggers notification)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(8,'2024-02-05','Absent'),(8,'2024-02-07','Present'),(8,'2024-02-12','Absent'),
(8,'2024-02-14','Present'),(8,'2024-02-19','Present'),(8,'2024-02-21','Absent'),
(8,'2024-02-26','Present'),(8,'2024-02-28','Present'),(8,'2024-03-04','Absent'),
(8,'2024-03-06','Present'),(8,'2024-03-11','Present'),(8,'2024-03-13','Present');

-- E10 Farhan Iqbal – CS-401 (excellent: 12/12)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(10,'2024-02-05','Present'),(10,'2024-02-07','Present'),(10,'2024-02-12','Present'),
(10,'2024-02-14','Present'),(10,'2024-02-19','Present'),(10,'2024-02-21','Present'),
(10,'2024-02-26','Present'),(10,'2024-02-28','Late'),   (10,'2024-03-04','Present'),
(10,'2024-03-06','Present'),(10,'2024-03-11','Present'),(10,'2024-03-13','Present');

-- E12 Bilal Ahmed – EE-101 (good: 9/10)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(12,'2024-02-05','Present'),(12,'2024-02-07','Present'),(12,'2024-02-12','Present'),
(12,'2024-02-14','Absent'), (12,'2024-02-19','Present'),(12,'2024-02-21','Present'),
(12,'2024-02-26','Present'),(12,'2024-02-28','Present'),(12,'2024-03-04','Present'),
(12,'2024-03-06','Present');

-- E13 Bilal Ahmed – EE-201 (good: 9/10)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(13,'2024-02-05','Present'),(13,'2024-02-07','Late'),   (13,'2024-02-12','Present'),
(13,'2024-02-14','Present'),(13,'2024-02-19','Present'),(13,'2024-02-21','Absent'),
(13,'2024-02-26','Present'),(13,'2024-02-28','Present'),(13,'2024-03-04','Present'),
(13,'2024-03-06','Present');

-- E16 Sana Tariq – BA-101 (good: 9/10)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(16,'2024-02-05','Present'),(16,'2024-02-07','Present'),(16,'2024-02-12','Present'),
(16,'2024-02-14','Present'),(16,'2024-02-19','Late'),   (16,'2024-02-21','Present'),
(16,'2024-02-26','Absent'), (16,'2024-02-28','Present'),(16,'2024-03-04','Present'),
(16,'2024-03-06','Present');

-- E17 Zara Khan – BA-101 (excellent: 10/10)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(17,'2024-02-05','Present'),(17,'2024-02-07','Present'),(17,'2024-02-12','Present'),
(17,'2024-02-14','Present'),(17,'2024-02-19','Present'),(17,'2024-02-21','Present'),
(17,'2024-02-26','Present'),(17,'2024-02-28','Present'),(17,'2024-03-04','Present'),
(17,'2024-03-06','Present');

-- E21 Tariq Mahmood – SE-101 (excellent: 12/12)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(21,'2024-02-05','Present'),(21,'2024-02-07','Present'),(21,'2024-02-12','Present'),
(21,'2024-02-14','Present'),(21,'2024-02-19','Present'),(21,'2024-02-21','Present'),
(21,'2024-02-26','Late'),   (21,'2024-02-28','Present'),(21,'2024-03-04','Present'),
(21,'2024-03-06','Present'),(21,'2024-03-11','Present'),(21,'2024-03-13','Present');

-- E23 Rida Fatima – SE-101 (good: 10/12)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(23,'2024-02-05','Present'),(23,'2024-02-07','Present'),(23,'2024-02-12','Absent'),
(23,'2024-02-14','Present'),(23,'2024-02-19','Present'),(23,'2024-02-21','Present'),
(23,'2024-02-26','Present'),(23,'2024-02-28','Present'),(23,'2024-03-04','Present'),
(23,'2024-03-06','Absent'), (23,'2024-03-11','Present'),(23,'2024-03-13','Present');

-- E25 Asad Mehmood – SE-101 (good: 10/12)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(25,'2024-02-05','Present'),(25,'2024-02-07','Present'),(25,'2024-02-12','Present'),
(25,'2024-02-14','Absent'), (25,'2024-02-19','Present'),(25,'2024-02-21','Present'),
(25,'2024-02-26','Present'),(25,'2024-02-28','Present'),(25,'2024-03-04','Absent'),
(25,'2024-03-06','Present'),(25,'2024-03-11','Present'),(25,'2024-03-13','Present');

-- E26 Asad Mehmood – SE-201 (good: 10/12)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(26,'2024-02-05','Present'),(26,'2024-02-07','Absent'), (26,'2024-02-12','Present'),
(26,'2024-02-14','Present'),(26,'2024-02-19','Present'),(26,'2024-02-21','Present'),
(26,'2024-02-26','Present'),(26,'2024-02-28','Present'),(26,'2024-03-04','Absent'),
(26,'2024-03-06','Present'),(26,'2024-03-11','Present'),(26,'2024-03-13','Present');

-- E29 Saad Abdullah – SE-201 (decent: 8/10)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(29,'2024-02-05','Present'),(29,'2024-02-07','Present'),(29,'2024-02-12','Absent'),
(29,'2024-02-14','Present'),(29,'2024-02-19','Present'),(29,'2024-02-21','Present'),
(29,'2024-02-26','Absent'), (29,'2024-02-28','Present'),(29,'2024-03-04','Present'),
(29,'2024-03-06','Present');

-- E30 Saad Abdullah – SE-301 (critical: 4/10 = 40% ? triggers notification)
INSERT INTO Attendance (EnrollmentID, AttendDate, Status) VALUES
(30,'2024-02-05','Absent'),(30,'2024-02-07','Absent'), (30,'2024-02-12','Present'),
(30,'2024-02-14','Absent'),(30,'2024-02-19','Present'),(30,'2024-02-21','Absent'),
(30,'2024-02-26','Present'),(30,'2024-02-28','Absent'),(30,'2024-03-04','Present'),
(30,'2024-03-06','Absent');
GO

-- ---- 8. RESULTS ---------------------------------------------
-- Uses verified EnrollmentIDs from the map above.
-- ResultID auto-increments 1..20.
INSERT INTO Results (EnrollmentID, Assignments, Midterm, FinalExam) VALUES
(1,  18.00, 25.00, 42.00),   -- R01  Ali Hassan       CS-101      85.0  ? A
(2,  19.50, 28.00, 45.00),   -- R02  Ali Hassan       MATH-101    92.5  ? A+
(3,  16.00, 22.00, 38.00),   -- R03  Ayesha           CS-101      76.0  ? B+
(4,  14.00, 19.00, 32.00),   -- R04  Ayesha           MATH-101    65.0  ? B-
(5,  15.00, 21.00, 36.00),   -- R05  Usman            CS-201      72.0  ? B
(6,  17.50, 24.00, 40.00),   -- R06  Usman            CS-301      81.5  ? A-
(8,  12.00, 18.00, 29.00),   -- R07  Hamza            CS-201      59.0  ? C
(10, 20.00, 29.00, 48.00),   -- R08  Farhan           CS-401      97.0  ? A+
(12, 17.00, 23.00, 39.00),   -- R09  Bilal            EE-101      79.0  ? B+
(13, 15.50, 20.00, 35.00),   -- R10  Bilal            EE-201      70.5  ? B
(16, 13.00, 18.50, 31.00),   -- R11  Sana             BA-101      62.5  ? C+
(17, 18.00, 26.00, 44.00),   -- R12  Zara             BA-101      88.0  ? A
(19, 16.50, 22.50, 37.00),   -- R13  Hina             BA-201      76.0  ? B+
(20, 14.00, 20.00, 33.00),   -- R14  Hina             BA-301      67.0  ? B-
(21, 19.00, 27.00, 46.00),   -- R15  Tariq Mahmood    SE-101      92.0  ? A+
(23, 17.00, 24.00, 41.00),   -- R16  Rida             SE-101      82.0  ? A-
(25, 16.00, 22.00, 38.50),   -- R17  Asad             SE-101      76.5  ? B+
(26, 18.50, 25.00, 43.00),   -- R18  Asad             SE-201      86.5  ? A
(29, 15.00, 21.00, 36.00),   -- R19  Saad             SE-201      72.0  ? B
(30, 10.00, 14.00, 22.00);   -- R20  Saad             SE-301      46.0  ? F
GO

-- ---- 9. ASSIGN GRADES (ResultID 1 through 20) ---------------
DECLARE @rid INT = 1;
WHILE @rid <= 20
BEGIN
    EXEC sp_AssignGrade @rid;
    SET @rid = @rid + 1;
END;
GO

-- ---- 10. MANUAL NOTIFICATIONS -------------------------------
INSERT INTO Notifications (UserID, Title, Message) VALUES
(1,  'Welcome to AcademixPro',   'Admin account is active and ready.'),
(3,  'Semester Schedule Posted', 'Timetable for Semester 1 is live. Please review your courses.'),
(9,  'Result Published',         'Your CS-101 result has been posted. Log in to check your grade.'),
(11, 'Enrollment Confirmed',     'You are enrolled in SE-101 and CS-101. Welcome to Software Engineering!'),
(12, 'Enrollment Confirmed',     'You are enrolled in SE-101 and MATH-101.'),
(13, 'Attendance Warning',       'Your CS-201 attendance is below the 75% threshold. Please attend regularly.'),
(15, 'Result Published',         'Your SE-301 result has been posted. Please review your performance.');
GO

PRINT 'AcademixProDB setup complete!';
PRINT '  Departments : 5  (CS, EE, BA, MATH, SE)';
PRINT '  Instructors : 8';
PRINT '  Courses     : 16';
PRINT '  Students    : 15';
PRINT '  Enrollments : 30  (one INSERT per row — no batch-abort risk)';
PRINT '  Attendance  : 17 blocks covering key enrollments';
PRINT '  Results     : 20  (grades A+ down to F)';
GO