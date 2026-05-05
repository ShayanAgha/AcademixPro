AcademixPro is a full-featured Academic Management System built with C# (.NET 8) and Windows Forms, backed by a SQL Server database.

It covers the complete lifecycle of academic administration: student and instructor management, course creation, enrollment (with stored procedures and transaction handling), attendance marking with a per-student Present/Absent/Late dropdown, result and grade entry with auto-calculated GPA, a notification centre, and a full database audit trail via triggers.

The dashboard provides live statistics — total students, instructors, courses, enrollments, and average CGPA — alongside a top-students leaderboard, recent enrollment feed, and a colour-coded department bar chart.

Key technical highlights:
• Role-based authentication (Admin / Teacher / Student) with SHA-256 password hashing
• All SQL encapsulated as constants in SqlQueries.cs; no inline SQL in UI code
• Stored procedures for enrollment (sp_EnrollStudent) and auto-grading (sp_AssignGrade)
• DB-level triggers for audit logging on the Students table
• Dark-themed, responsive WinForms UI with a shared colour palette and UIHelper factory

Tech stack: C# 12 · .NET 8 (net8.0-windows) · Windows Forms · SQL Server · ADO.NET
