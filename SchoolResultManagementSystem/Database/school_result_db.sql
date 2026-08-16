CREATE DATABASE IF NOT EXISTS school_result_db;
USE school_result_db;

CREATE TABLE IF NOT EXISTS users (
    user_id     INT AUTO_INCREMENT PRIMARY KEY,
    username    VARCHAR(50)  NOT NULL UNIQUE,
    password    VARCHAR(64)  NOT NULL,
    full_name   VARCHAR(100) NOT NULL,
    role        ENUM('Teacher', 'Student') NOT NULL,
    created_at  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS students (
    student_id  INT AUTO_INCREMENT PRIMARY KEY,
    user_id     INT NOT NULL,
    roll_no     VARCHAR(20) NOT NULL UNIQUE,
    class_name  VARCHAR(20) NOT NULL,
    section     VARCHAR(10) NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS subjects (
    subject_id   INT AUTO_INCREMENT PRIMARY KEY,
    subject_name VARCHAR(100) NOT NULL UNIQUE,
    max_marks    INT NOT NULL DEFAULT 100
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS results (
    result_id       INT AUTO_INCREMENT PRIMARY KEY,
    student_id      INT NOT NULL,
    subject_id      INT NOT NULL,
    exam_term       VARCHAR(30) NOT NULL,
    marks_obtained  DECIMAL(5,2) NOT NULL,
    recorded_at     TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (student_id) REFERENCES students(student_id) ON DELETE CASCADE,
    FOREIGN KEY (subject_id) REFERENCES subjects(subject_id) ON DELETE CASCADE,
    UNIQUE KEY uq_student_subject_term (student_id, subject_id, exam_term)
) ENGINE=InnoDB;

INSERT INTO users (username, password, full_name, role) VALUES
    ('admin',    '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Naghma Abid', 'Teacher'),
    ('student1', '703b0a3d6ad75b649a28adde7d83c6251da457549263bc7ff45ec709b0a8448b', 'Ali Raza',     'Student'),
    ('student2', '703b0a3d6ad75b649a28adde7d83c6251da457549263bc7ff45ec709b0a8448b', 'Sara Khan',    'Student');

INSERT INTO students (user_id, roll_no, class_name, section) VALUES
    ((SELECT user_id FROM users WHERE username = 'student1'), 'R-1001', '10', 'A'),
    ((SELECT user_id FROM users WHERE username = 'student2'), 'R-1002', '10', 'A');

INSERT INTO subjects (subject_name, max_marks) VALUES
    ('Mathematics', 100),
    ('Computer Science', 100),
    ('English', 100),
    ('Physics', 100);

INSERT INTO results (student_id, subject_id, exam_term, marks_obtained) VALUES
    (1, 1, 'Final', 88),
    (1, 2, 'Final', 95),
    (1, 3, 'Final', 76),
    (1, 4, 'Final', 82),
    (2, 1, 'Final', 67),
    (2, 2, 'Final', 91),
    (2, 3, 'Final', 84),
    (2, 4, 'Final', 73);
