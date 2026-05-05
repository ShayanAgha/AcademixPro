# AcademixPro

> A modern, full-featured **Academic Management System** built with **C# Windows Forms** and **SQL Server**, designed to streamline the administration of students, instructors, courses, attendance, and results in an educational institution.

---

## 📸 Overview

AcademixPro provides a clean, dark-themed desktop interface for managing every aspect of an academic institution — from student registration and course enrollment to grade entry, attendance tracking, and audit logging — all backed by a robust SQL Server database with stored procedures, triggers, views, and transactions.

---

## ✨ Features

### 🔐 Authentication & Role Management
- Secure login with **SHA-256 password hashing**
- Role-based access control: **Admin**, **Teacher**, **Student**
- User account management — create, activate/deactivate accounts
- Session tracking with last login timestamps

### 📊 Dashboard
- Real-time stats: Total Students, Instructors, Active Courses, Enrollments, Average CGPA
- **Top 5 performing students** table
- **Recent enrollments** feed
- **Department enrollment bar chart** (colour-coded per department)

### 👨‍🎓 Student Management
- Add, update, and deactivate student records
- Auto-generated unique Student Codes (`STU-YYYY-NNN`)
- Search by Student ID code
- Fields: Full Name, Email, Phone, Gender, Date of Birth, Address, Department, Semester, Admission Date

### 🧑‍🏫 Instructor Management
- Add, update, and deactivate instructors
- Auto-generated Instructor Codes (`INS-NNN`)
- Track specialization, department, joining date, and courses assigned

### 📚 Course Management
- Create and manage courses with course code, credit hours, capacity, semester, and instructor assignment
- View current enrollment counts per course
- Deactivate courses without data loss

### 🔗 Enrollment Management
- Enroll students in available courses via stored procedure (`sp_EnrollStudent`)
- Prevents duplicate enrollments and over-capacity via DB-level constraints
- Drop course functionality with confirmation

### 🗓️ Attendance Management
- Select a course and date to mark attendance
- Per-student **Present / Absent / Late** dropdown selection
- View full attendance history per course
- Attendance summary with percentage per student
- Auto-notifications triggered when attendance drops below 75%

### 📝 Results & Grading
- Enter marks for Assignments (0–20), Midterm (0–30), and Final Exam (0–50)
- Grades auto-calculated via stored procedure (`sp_AssignGrade`)
- GPA and total marks computed automatically
- Lock results to prevent post-finalization edits

### 🔔 Notifications
- In-app notification centre for unread messages
- Send custom notifications to users
- System auto-generates alerts for low attendance

### 🧾 Audit Log
- Tracks all **INSERT**, **UPDATE**, and **DELETE** operations on the Students table via database triggers
- View action history with timestamps and changed-by info

### 🏛️ Department Management
- Manage academic departments with name, code, and description
- Department data feeds into student and course dropdowns

---

## 🗄️ Database Architecture

The SQL Server database includes:

| Feature | Details |
|---------|---------|
| **Stored Procedures** | `sp_EnrollStudent`, `sp_AssignGrade` — safe, parameterized operations |
| **Triggers** | Auto audit logging on Students table (INSERT, UPDATE, DELETE) |
| **Transactions** | Enrollment uses transactions for atomicity |
| **Views** | Simplified query surfaces for reporting |
| **Constraints** | Unique student codes, capacity limits, grade boundaries |
| **Indexes** | Optimized for frequent search queries |

---

## 🗂️ Project Structure

```
AcademixPro/
│
├── Forms/                          ← All UI screens (UserControls + Forms)
│   ├── LoginForm.cs                ← Secure login screen
│   ├── MainForm.cs                 ← Navigation shell
│   ├── DashboardControl.cs         ← Live stats & charts
│   ├── StudentsControl.cs          ← Student CRUD + search
│   ├── InstructorsControl.cs       ← Instructor CRUD
│   ├── CoursesControl.cs           ← Course management
│   ├── DepartmentsControl.cs       ← Department management
│   ├── EnrollmentsControl.cs       ← Enroll / drop courses
│   ├── AttendanceControl.cs        ← Mark & view attendance
│   ├── ResultsControl.cs           ← Grade entry & locking
│   ├── UsersControl.cs             ← User account management
│   ├── NotificationsControl.cs     ← Notification centre
│   └── AuditLogControl.cs          ← System audit trail
│
├── Helpers/                        ← Shared utilities
│   ├── AppColors.cs                ← Centralized colour palette
│   ├── Session.cs                  ← Login session state
│   └── UIHelper.cs                 ← Shared UI factory methods
│
├── Database/                       ← Data access layer
│   ├── DatabaseHelper.cs           ← SQL execution wrapper
│   ├── SqlQueries.cs               ← All SQL query constants
│   └── DatabaseSetup.sql           ← Full DB schema + seed data
│
├── Requirements/                   ← Project documentation
│   ├── Features.md                 ← Epic & user story breakdown
│   └── TestPlans.md                ← Test plan documentation
│
├── Program.cs                      ← Application entry point
└── AcademixPro.csproj              ← .NET project file
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Language** | C# 12 |
| **Framework** | .NET 8 (net8.0-windows) |
| **UI** | Windows Forms (WinForms) |
| **Database** | Microsoft SQL Server |
| **ORM / Data Access** | ADO.NET (`System.Data.SqlClient`) |
| **IDE** | Visual Studio 2022 |

---

## ⚙️ Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or higher)
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) *(optional but recommended)*
- Windows OS (WinForms is Windows-only)

---

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/ShayanAgha/AcademixPro.git
cd AcademixPro
```

### 2. Set up the database

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Open and run the full script: `Database/DatabaseSetup.sql`
   - This creates the database, all tables, stored procedures, triggers, views, and seed data

### 3. Configure the connection string

Open `Database/DatabaseHelper.cs` and update the connection string to match your SQL Server instance:

```csharp
private const string ConnectionString =
    "Server=YOUR_SERVER_NAME;Database=AcademixPro;Trusted_Connection=True;";
```

> Replace `YOUR_SERVER_NAME` with your SQL Server instance name (e.g. `localhost`, `.\SQLEXPRESS`).

### 4. Build & run

```bash
dotnet build
dotnet run
```

Or open `AcademixPro.sln` in **Visual Studio 2022** and press **F5**.

### 5. Default Login

After running the database setup script, use the seeded admin credentials:

| Field | Value |
|-------|-------|
| **Username** | `admin` |
| **Password** | `admin123` |

---

## 📋 Default Roles

| Role | Access |
|------|--------|
| **Admin** | Full system access |
| **Teacher** | Mark attendance, enter results |
| **Student** | View-only (future extension) |

---

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch: `git checkout -b feature/your-feature-name`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Open a Pull Request

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Shayan Agha**
- GitHub: [@ShayanAgha](https://github.com/ShayanAgha)

---

*Built with ❤️ using C# and Windows Forms*
