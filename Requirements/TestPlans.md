What you’ve built is already quite advanced—so a generic “test cases list” won’t cut it. You need a **structured, DBMS-focused, multi-layer test plan** that matches your epics, advanced features (triggers, transactions, MFA, analytics), and dashboard interactivity.

I’ll give you a **complete, submission-ready Test Plan**.

---

# 🧪 📘 TEST PLAN – UNIVERSITY MANAGEMENT SYSTEM

---

# 🎯 1. Test Plan Objective

To verify that the system:

* Meets all functional and non-functional requirements
* Ensures **data integrity, security, and performance**
* Correctly implements **DBMS concepts (transactions, triggers, constraints, indexing)**
* Provides accurate analytics and real-time dashboard updates

---

# 🧱 2. Testing Scope

### Included:

* All 18 Epics
* Database operations (CRUD + advanced)
* Dashboard interactivity
* Security mechanisms (MFA, roles)
* Performance & optimization

### Excluded:

* External integrations (if any)
* Hardware/network failures (basic level only)

---

# 🧪 3. Testing Types

## ✅ Functional Testing

* All user stories per epic

## 🗄️ Database Testing (CRITICAL)

* Constraints validation
* Stored procedures
* Triggers
* Transactions (ACID)

## 🔐 Security Testing

* Authentication & authorization
* MFA
* SQL Injection prevention

## ⚡ Performance Testing

* Query response time (<100ms target)
* Concurrent users

## 🎨 UI Testing

* Dashboard filters, search, expand/collapse

## 🔄 Integration Testing

* C# ↔ SQL Server connectivity

---

# 📊 4. Test Strategy

* **Black-box testing** → UI & workflows
* **White-box testing** → DB logic (triggers, SPs)
* **Unit testing** → Individual modules
* **Integration testing** → Combined modules

---

# 📋 5. TEST CASES BY EPIC

---

# 🔐 EPIC 1: User & Access Management

### Test Cases:

1. Login with correct credentials → Success
2. Login with wrong password → Error
3. Password stored as hash (not plain text)
4. MFA enabled → OTP required
5. Blocked user cannot login
6. IP logging recorded correctly

---

# 👨‍🎓 EPIC 2: Student Management

### Test Cases:

1. Add student → Record inserted
2. Duplicate email → Rejected (UNIQUE constraint)
3. Update student → Version tracking created
4. Delete → Archive instead of permanent delete
5. Search student → Correct results

---

# 🧑‍🏫 EPIC 3: Instructor Management

### Test Cases:

1. Add instructor → Stored correctly
2. Assign course → Relationship created
3. Conflict detection → Prevent overlapping schedule

---

# 📚 EPIC 4: Course Management

### Test Cases:

1. Add course → Success
2. Assign prerequisite → Enforced
3. Course capacity limit → Cannot exceed

---

# 🔗 EPIC 5: Enrollment Management

### Test Cases:

1. Enroll student → Success
2. Duplicate enrollment → Blocked
3. Seat full → Enrollment denied
4. Transaction failure → Rollback occurs
5. Add/drop window → Enforced

---

# 🗓️ EPIC 6: Attendance

### Test Cases:

1. Mark attendance → Saved
2. Update attendance → Updated correctly
3. Attendance % auto-calculated (trigger)
4. <75% → Alert generated

---

# 📝 EPIC 7: Results

### Test Cases:

1. Enter marks → Stored
2. Grade auto-calculated (trigger)
3. GPA calculated correctly
4. Result locked → No further edits

---

# 📊 EPIC 8: Reporting & Analytics

### Test Cases:

1. Generate report → Accurate data
2. View-based query → Matches raw data
3. At-risk students identified correctly
4. Grade distribution accurate

---

# 🔍 EPIC 9: Search & Filtering

### Test Cases:

1. Search by ID → Correct result
2. Filter by semester → Correct subset
3. Indexed search → Faster response

---

# 🔔 EPIC 10: Notifications

### Test Cases:

1. Low attendance alert triggered
2. Result announcement displayed
3. Enrollment confirmation shown

---

# 🧠 EPIC 11: Database Core

### Test Cases:

1. Stored procedure executes correctly
2. Trigger fires on insert/update
3. Constraints reject invalid data
4. Views return correct data

---

# 🔄 EPIC 12: Transactions

### Test Cases:

1. Multi-step operation success → Commit
2. Failure → Rollback
3. Partial updates prevented

---

# 🔐 EPIC 13: Security

### Test Cases:

1. Role-based access enforced
2. Unauthorized access blocked
3. SQL injection attempt → Prevented

---

# 🧾 EPIC 14: Audit & Logging

### Test Cases:

1. Insert operation logged
2. Update operation logged
3. Logs contain timestamp & user

---

# 📦 EPIC 15: Backup & Recovery

### Test Cases:

1. Backup created successfully
2. Restore returns correct data

---

# ⚡ EPIC 16: Performance

### Test Cases:

1. Query execution <100ms
2. Index improves performance
3. No N+1 query issue

---

# 🎨 EPIC 17: UI Dashboard

### Test Cases:

1. Search bar filters correctly
2. Actor filter works
3. Clicking epic expands stories
4. Summary stats update dynamically

---

# 🧪 EPIC 18: Testing & Validation

### Test Cases:

1. Edge cases handled
2. Invalid input rejected
3. System does not crash

---

# 📈 6. PERFORMANCE BENCHMARKS

* Query response time: **<100 ms**
* Concurrent users: **10–50 users (academic level)**
* Dashboard load time: **<2 seconds**

---

# 🧰 7. TEST DATA

* Sample students (100+ records)
* Courses (10–20)
* Attendance logs
* Marks dataset

---

# ⚠️ 8. RISK AREAS

* Transaction failures
* Trigger misfires
* Data inconsistency
* Performance bottlenecks

---

# ✅ 9. ACCEPTANCE CRITERIA

System is accepted if:

* All test cases pass
* No critical bugs
* DB integrity maintained
* Performance benchmarks met

