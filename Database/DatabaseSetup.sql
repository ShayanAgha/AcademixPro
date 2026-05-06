use master;

if not exists (select name from sys.databases where name = 'academixprodb')
    create database academixprodb;

use academixprodb;

if object_id('notifications', 'u') is not null drop table notifications;
if object_id('auditlog', 'u') is not null drop table auditlog;
if object_id('results', 'u') is not null drop table results;
if object_id('attendance', 'u') is not null drop table attendance;
if object_id('enrollments', 'u') is not null drop table enrollments;
if object_id('courses', 'u') is not null drop table courses;
if object_id('students', 'u') is not null drop table students;
if object_id('instructors', 'u') is not null drop table instructors;
if object_id('departments', 'u') is not null drop table departments;
if object_id('users', 'u') is not null drop table users;
go

create table departments (
    deptid int identity(1,1) primary key,
    deptname nvarchar(100) not null unique,
    deptcode nvarchar(10)  not null unique,
    description nvarchar(255),
    createdat datetime default getdate()
);
go

create table users (
    userid int identity(1,1) primary key,
    username nvarchar(50)  not null unique,
    passwordhash nvarchar(256) not null,
    fullname nvarchar(100) not null,
    email nvarchar(150) not null unique,
    role nvarchar(20)  not null check (role in ('admin','teacher','student')),
    isactive bit default 1,
    lastlogin datetime,
    createdat datetime default getdate()
);
go

create table students (
    studentid int identity(1,1) primary key,
    studentcode nvarchar(20)  not null unique,
    fullname nvarchar(100) not null,
    email nvarchar(150) not null unique,
    phone nvarchar(20),
    dateofbirth date,
    gender nvarchar(10)  check (gender in ('male','female','other')),
    address nvarchar(255),
    deptid int references departments(deptid),
    admissiondate date default getdate(),
    semester int  check (semester between 1 and 12),
    cgpa decimal(4,2) default 0.00,
    isactive bit default 1,
    createdat datetime default getdate()
);
go

create table instructors (
    instructorid int identity(1,1) primary key,
    instructorcode nvarchar(20)  not null unique,
    fullname nvarchar(100) not null,
    email nvarchar(150) not null unique,
    phone nvarchar(20),
    specialization nvarchar(100),
    deptid int references departments(deptid),
    joiningdate date default getdate(),
    isactive bit default 1,
    createdat datetime default getdate()
);
go

create table courses (
    courseid int identity(1,1) primary key,
    coursecode nvarchar(20)  not null unique,
    coursename nvarchar(150) not null,
    credithours int not null check (credithours between 1 and 6),
    deptid int references departments(deptid),
    instructorid int references instructors(instructorid),
    semester int check (semester between 1 and 12),
    capacity int default 40,
    isactive bit default 1,
    createdat datetime default getdate()
);
go

create table enrollments (
    enrollmentid int identity(1,1) primary key,
    studentid int not null references students(studentid),
    courseid int not null references courses(courseid),
    enrolldate date default getdate(),
    status nvarchar(20) default 'active'
                   check (status in ('active','dropped','completed')),
    constraint uq_enrollment unique (studentid, courseid)
);
go

create table attendance (
    attendanceid int identity(1,1) primary key,
    enrollmentid int not null references enrollments(enrollmentid),
    attenddate date not null default getdate(),
    status nvarchar(10) not null check (status in ('present','absent','late')),
    markedby int references users(userid),
    remarks nvarchar(200),
    constraint uq_attendance unique (enrollmentid, attenddate)
);
go

create table results (
    resultid int identity(1,1) primary key,
    enrollmentid int not null references enrollments(enrollmentid) unique,
    assignments decimal(5,2) default 0 check (assignments between 0 and 20),
    midterm decimal(5,2) default 0 check (midterm     between 0 and 30),
    finalexam decimal(5,2) default 0 check (finalexam   between 0 and 50),
    totalmarks as (assignments + midterm + finalexam),
    grade nvarchar(5),
    gradepoints decimal(3,2),
    islocked bit default 0,
    enteredby int references users(userid),
    enteredat datetime default getdate()
);
go

create table notifications (
    notifid int identity(1,1) primary key,
    userid int references users(userid),
    title nvarchar(100) not null,
    message nvarchar(500) not null,
    isread bit default 0,
    createdat datetime default getdate()
);
go

create table auditlog (
    logid int identity(1,1) primary key,
    tablename nvarchar(50)  not null,
    action nvarchar(10)  not null,
    recordid int,
    changedby nvarchar(100),
    changetime datetime default getdate(),
    details nvarchar(500)
);
go

--  stored procedures
create or alter procedure sp_enrollstudent
    @studentid int,
    @courseid  int
as
begin
    set nocount on;
    begin try
        begin transaction;
        if exists (select 1 from enrollments where studentid=@studentid and courseid=@courseid)
        begin raiserror('student is already enrolled in this course.',16,1); rollback; return; end

        declare @capacity int, @enrolled int;
        select @capacity = capacity from courses where courseid = @courseid;
        select @enrolled = count(*) from enrollments where courseid=@courseid and status='active';
        if @enrolled >= @capacity
        begin raiserror('course has reached maximum capacity.',16,1); rollback; return; end

        insert into enrollments (studentid, courseid) values (@studentid, @courseid);
        commit;
        select 'enrollment successful.' as result;
    end try
    begin catch
        if @@trancount > 0 rollback; throw;
    end catch
end;
go

create or alter procedure sp_assigngrade
    @resultid int
as
begin
    declare @total decimal(5,2);
    select @total = totalmarks from results where resultid = @resultid;

    declare @grade nvarchar(5), @gpa decimal(3,2);
    select
        @grade = case
            when @total >= 90 then 'a+' when @total >= 85 then 'a'
            when @total >= 80 then 'a-' when @total >= 75 then 'b+'
            when @total >= 70 then 'b' when @total >= 65 then 'b-'
            when @total >= 60 then 'c+' when @total >= 55 then 'c'
            when @total >= 50 then 'd' else 'f'
        end,
        @gpa = case
            when @total >= 90 then 4.0 when @total >= 85 then 4.0
            when @total >= 80 then 3.7 when @total >= 75 then 3.3
            when @total >= 70 then 3.0 when @total >= 65 then 2.7
            when @total >= 60 then 2.3 when @total >= 55 then 2.0
            when @total >= 50 then 1.0 else 0.0
        end;

    update results set grade = @grade, gradepoints = @gpa where resultid = @resultid;

    declare @studentid int;
    select @studentid = e.studentid
    from results r join enrollments e on r.enrollmentid = e.enrollmentid
    where r.resultid = @resultid;

    update students set cgpa = (
        select round(avg(r2.gradepoints), 2)
        from results r2 join enrollments e2 on r2.enrollmentid = e2.enrollmentid
        where e2.studentid = @studentid and r2.gradepoints is not null
    ) where studentid = @studentid;
end;
go

create or alter procedure sp_getattendancepercentage
    @enrollmentid int
as
begin
    select
        count(*) as totalclasses,
        sum(case when status in ('present','late') then 1 else 0 end) as attended,
        cast(
            sum(case when status in ('present','late') then 1 else 0 end) * 100.0
            / nullif(count(*), 0) as decimal(5,2)
        ) as attendancepercent
    from attendance where enrollmentid = @enrollmentid;
end;
go

create or alter procedure sp_authenticateuser
    @username     nvarchar(50),
    @passwordhash nvarchar(256)
as
begin
    select userid, username, fullname, email, role, isactive
    from users
    where username=@username and passwordhash=@passwordhash and isactive=1;

    update users set lastlogin=getdate()
    where username=@username and passwordhash=@passwordhash;
end;
go

--  triggers
create or alter trigger trg_students_audit
on students after insert, update, delete
as
begin
    set nocount on;
    if exists(select 1 from inserted)
        insert into auditlog(tablename, action, recordid, details)
        select 'students',
               case when exists(select 1 from deleted) then 'update' else 'insert' end,
               studentid, 'student record modified: ' + fullname
        from inserted;
    else
        insert into auditlog(tablename, action, recordid, details)
        select 'students','delete',studentid,'student deleted: '+fullname from deleted;
end;
go

create or alter trigger trg_lowattendance
on attendance after insert
as
begin
    set nocount on;
    declare @enrollmentid int, @pct decimal(5,2);
    select @enrollmentid = enrollmentid from inserted;

    select @pct = cast(
        sum(case when status in ('present','late') then 1 else 0 end) * 100.0
        / nullif(count(*),0) as decimal(5,2))
    from attendance where enrollmentid = @enrollmentid;

    if @pct < 75
    begin
        declare @studentid int;
        select @studentid = e.studentid from enrollments e where e.enrollmentid=@enrollmentid;
        if not exists (
            select 1 from notifications
            where userid=@studentid and title='low attendance warning'
              and cast(createdat as date) = cast(getdate() as date)
        )
        insert into notifications(userid, title, message)
        select s.studentid,
               'low attendance warning',
               'your attendance has dropped to '+cast(@pct as nvarchar)+'%. minimum required is 75%.'
        from students s where s.studentid=@studentid;
    end
end;
go

--  views
create or alter view vw_studentdashboard as
select s.studentid, s.studentcode, s.fullname as studentname,
       s.email, s.semester, s.cgpa, d.deptname as department,
       count(e.enrollmentid) as coursesenrolled
from students s
left join departments d on s.deptid = d.deptid
left join enrollments e on s.studentid = e.studentid and e.status = 'active'
where s.isactive = 1
group by s.studentid, s.studentcode, s.fullname, s.email, s.semester, s.cgpa, d.deptname;
go

create or alter view vw_courseenrollmentstatus as
select c.courseid, c.coursecode, c.coursename, c.credithours, c.capacity,
       count(e.enrollmentid) as enrolledcount,
       c.capacity - count(e.enrollmentid) as seatsavailable,
       i.fullname as instructorname, d.deptname
from courses c
left join enrollments e on c.courseid = e.courseid and e.status = 'active'
left join instructors i on c.instructorid = i.instructorid
left join departments d on c.deptid = d.deptid
where c.isactive = 1
group by c.courseid, c.coursecode, c.coursename, c.credithours, c.capacity, i.fullname, d.deptname;
go

create or alter view vw_resultsheet as
select s.studentcode, s.fullname as studentname,
       c.coursecode, c.coursename,
       r.assignments, r.midterm, r.finalexam,
       r.totalmarks, r.grade, r.gradepoints, r.islocked, d.deptname
from results r
join enrollments e on r.enrollmentid = e.enrollmentid
join students s on e.studentid = s.studentid
join courses c on e.courseid = c.courseid
join departments d on s.deptid = d.deptid;
go

create or alter view vw_attendancesummary as
select s.studentcode, s.fullname as studentname, c.coursename,
       count(*) as totalclasses,
       sum(case when a.status in ('present','late') then 1 else 0 end) as attended,
       cast(sum(case when a.status in ('present','late') then 1 else 0 end)*100.0
            / nullif(count(*),0) as decimal(5,2)) as attendancepercent
from attendance a
join enrollments e on a.enrollmentid = e.enrollmentid
join students s on e.studentid = s.studentid
join courses c on e.courseid = c.courseid
group by s.studentcode, s.fullname, c.coursename;
go