-- Create the database
CREATE DATABASE ProgPart3;
GO

USE ProgPart3;
GO

-- ============================================================
-- TASKS TABLE - Stores user tasks
-- ============================================================
CREATE TABLE Tasks(
    task_id INT IDENTITY(1,1) PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    reminder_date DATETIME,
    is_completed BIT DEFAULT 0,
    created_at DATETIME DEFAULT GETDATE()
);
GO

-- ============================================================
-- ACTIVITY LOG TABLE - Stores all user actions
-- ============================================================
CREATE TABLE ActivityLog(
    log_id INT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(50),
    action VARCHAR(255) NOT NULL,
    details TEXT,
    timestamp DATETIME DEFAULT GETDATE()
);
GO

-- ============================================================
-- QUIZ RESULTS TABLE - Stores quiz scores
-- ============================================================
CREATE TABLE QuizResults(
    result_id INT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(50),
    score INT,
    total_questions INT,
    percentage DECIMAL(5,2),
    completion_date DATETIME DEFAULT GETDATE()
);
GO

-- ============================================================
-- TEST: View empty tables to verify they exist
-- ============================================================
SELECT * FROM Tasks;
SELECT * FROM ActivityLog;
SELECT * FROM QuizResults;

-- ============================================================
-- TEST: Insert sample data to verify database works
-- ============================================================
INSERT INTO ActivityLog (username, action, details) 
VALUES ('TestUser', 'Database Test', 'Connection successful');
GO

SELECT * FROM ActivityLog;
GO

-- Clean up test data
DELETE FROM ActivityLog WHERE username = 'TestUser';
GO