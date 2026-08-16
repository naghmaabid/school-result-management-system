# School Result Management System

A desktop application built with **C# Windows Forms (.NET Framework)** and a **MySQL**
backend (via **XAMPP**) to manage student records and examination results, with separate
role-based functionality for **teachers** and **students**.

## Features

- **Secure login** with role selection (Teacher / Student), passwords hashed with SHA-256
- **Teacher tools:**
  - Add, edit, and delete student records
  - Add, edit, and delete subjects
  - Enter or update exam marks per student, per subject, per term (Midterm/Final)
  - View all results with class-based filtering
  - Generate a report card for any student (total, percentage, grade)
- **Student view:**
  - Log in and see only their own results — as a read-only report card with
    percentage and grade automatically calculated
- **Data integrity:** foreign keys with cascading deletes, unique constraints so a
  student can't have two marks entries for the same subject/term

## Tech Stack

| Layer | Technology |
|---|---|
| UI | C# Windows Forms (.NET Framework 4.7.2) |
| Backend logic | C# |
| Database | MySQL (via XAMPP) |
| DB Access | MySql.Data (NuGet) |

## Project Structure

```
SchoolResultManagementSystem/
├── SchoolResultManagementSystem.sln
├── SchoolResultManagementSystem/
│   ├── Program.cs                     # App entry point
│   ├── App.config                     # MySQL connection string
│   ├── Helpers/
│   │   ├── DatabaseHelper.cs          # Centralized MySQL query/connection logic
│   │   ├── PasswordHelper.cs          # SHA-256 password hashing
│   │   └── GradeHelper.cs             # Percentage → letter grade
│   ├── Models/                        # Student, Subject, User, ResultRecord
│   └── Forms/
│       ├── LoginForm.cs               # Role-based login
│       ├── TeacherDashboardForm.cs
│       ├── StudentDashboardForm.cs    # Read-only report card for students
│       ├── ManageStudentsForm.cs      # Student CRUD
│       ├── ManageSubjectsForm.cs      # Subject CRUD
│       ├── EnterResultsForm.cs        # Enter/update marks
│       └── ViewAllResultsForm.cs      # Filterable results + report card generator
└── Database/
    └── school_result_db.sql           # Full schema + demo seed data
```

## Setup

### 1. Database (XAMPP / MySQL)

1. Start **Apache** and **MySQL** in the XAMPP Control Panel.
2. Open **phpMyAdmin** → **Import** → select `Database/school_result_db.sql` → **Go**.
   (Or run it via the MySQL CLI: `mysql -u root -p < Database/school_result_db.sql`)
3. This creates the `school_result_db` database, all tables, and demo accounts.

### 2. Application (Visual Studio)

1. Open `SchoolResultManagementSystem.sln` in Visual Studio (2019 or later recommended).
2. Restore NuGet packages: right-click the solution → **Restore NuGet Packages**
   (installs `MySql.Data`, referenced in `packages.config`).
3. If your MySQL root user has a password, or MySQL isn't on the default port, update
   the connection string in `App.config`:
   ```xml
   <add name="SchoolResultDb"
        connectionString="Server=localhost;Port=3306;Database=school_result_db;Uid=root;Pwd=;"
        providerName="MySql.Data.MySqlClient" />
   ```
4. Press **F5** (or **Start**) to build and run.

### Demo Logins

| Role | Username | Password |
|---|---|---|
| Teacher | `admin` | `admin123` |
| Student | `student1` | `student123` |
| Student | `student2` | `student123` |

## How It Works

- All SQL access goes through `DatabaseHelper`, so no form talks to `MySqlConnection`
  directly — keeps connection handling and query execution in one place.
- Passwords are hashed with SHA-256 before being stored or compared; nothing is ever
  saved in plain text.
- Teachers see full CRUD screens for students, subjects, and results. Students only
  ever see their own row of data, enforced by filtering results on their logged-in
  `user_id` — there's no way for a student account to query anyone else's marks.
- Deleting a student cascades (via SQL foreign keys) to remove their login and all
  their result records automatically, keeping the database consistent.

## License

MIT — see [LICENSE](LICENSE).
