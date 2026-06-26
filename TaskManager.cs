using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CyberBotGUI
{
    public class TaskManager
    {
        // Connection string for LocalDB - change this to match your SQL Server
        private string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ProgPart3;Integrated Security=True;";

        // In-memory activity log (for quick access and fallback)
        public List<string> ActivityLog = new List<string>();
        public string CurrentUser = "";

        public TaskManager()
        {
            // Tables are created manually using the SQL script
            // This ensures the database structure exists before we try to use it
        }

        // ============================================================
        // LOGGING METHODS - Save user actions to database
        // ============================================================

        // Log user action to database and memory
        public void LogAction(string action, string details = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO ActivityLog (username, action, details) 
                                   VALUES (@username, @action, @details)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", string.IsNullOrEmpty(CurrentUser) ? "Guest" : CurrentUser);
                    cmd.Parameters.AddWithValue("@action", action);
                    cmd.Parameters.AddWithValue("@details", details);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // Log success for debugging
                    Debug.WriteLine("LogAction saved: " + action + " (Rows: " + rowsAffected + ")");
                }
            }
            catch (Exception ex)
            {
                // If database fails, log the error for debugging
                Debug.WriteLine("LogAction database error: " + ex.Message);
            }

            // Always keep an in-memory copy too
            string logEntry = action + (string.IsNullOrEmpty(details) ? "" : ": " + details);
            ActivityLog.Add(logEntry);
        }

        // Save quiz result to database
        public void SaveQuizResult(string username, int score, int totalQuestions, double percentage)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO QuizResults (username, score, total_questions, percentage) 
                                   VALUES (@username, @score, @totalQuestions, @percentage)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", string.IsNullOrEmpty(username) ? "Guest" : username);
                    cmd.Parameters.AddWithValue("@score", score);
                    cmd.Parameters.AddWithValue("@totalQuestions", totalQuestions);
                    cmd.Parameters.AddWithValue("@percentage", percentage);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    Debug.WriteLine("QuizResult saved: " + username + " - Score: " + score + "/" + totalQuestions + " (Rows: " + rowsAffected + ")");

                    LogAction("Quiz Completed", "Score: " + score + "/" + totalQuestions + " (" + percentage.ToString("F0") + "%)");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveQuizResult database error: " + ex.Message);
                LogAction("Quiz Completed (Offline)", "Score: " + score + "/" + totalQuestions);
            }
        }

        // ============================================================
        // TASK METHODS - CRUD operations for tasks
        // ============================================================

        // Add a task to the database with optional reminder
        public bool AddTask(string title, string description = "", DateTime? reminderDate = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO Tasks (title, description, reminder_date) 
                                   VALUES (@title, @description, @reminderDate)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                    cmd.Parameters.AddWithValue("@reminderDate", (object)reminderDate ?? DBNull.Value);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    Debug.WriteLine("AddTask saved: " + title + " (Rows: " + rowsAffected + ")");

                    string details = "Task: '" + title + "'";
                    if (reminderDate.HasValue)
                        details += " (Reminder: " + reminderDate.Value.ToShortDateString() + ")";

                    LogAction("Task Added", details);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AddTask database error: " + ex.Message);
                return false;
            }
        }

        // Get all tasks from the database
        public List<TaskItem> GetAllTasks()
        {
            List<TaskItem> tasks = new List<TaskItem>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Tasks ORDER BY created_at DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        tasks.Add(new TaskItem
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            ReminderDate = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                            IsCompleted = reader.GetBoolean(4),
                            CreatedAt = reader.GetDateTime(5)
                        });
                    }

                    Debug.WriteLine("GetAllTasks retrieved: " + tasks.Count + " tasks");
                    LogAction("Viewed Tasks", "Total: " + tasks.Count + " tasks");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAllTasks database error: " + ex.Message);
            }
            return tasks;
        }

        // Mark a task as completed
        public bool CompleteTask(int taskId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // First get the task title for logging
                    string getTitle = "SELECT title FROM Tasks WHERE task_id = @id";
                    SqlCommand getCmd = new SqlCommand(getTitle, conn);
                    getCmd.Parameters.AddWithValue("@id", taskId);
                    object titleResult = getCmd.ExecuteScalar();

                    if (titleResult == null)
                    {
                        Debug.WriteLine("CompleteTask: Task " + taskId + " not found");
                        return false;
                    }

                    string title = titleResult.ToString();

                    // Then update the task
                    string query = "UPDATE Tasks SET is_completed = 1 WHERE task_id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", taskId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    Debug.WriteLine("CompleteTask: Task " + taskId + " completed (Rows: " + rowsAffected + ")");
                    LogAction("Task Completed", "'" + title + "'");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CompleteTask database error: " + ex.Message);
                return false;
            }
        }

        // Delete a task
        public bool DeleteTask(int taskId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // First get the task title for logging
                    string getTitle = "SELECT title FROM Tasks WHERE task_id = @id";
                    SqlCommand getCmd = new SqlCommand(getTitle, conn);
                    getCmd.Parameters.AddWithValue("@id", taskId);
                    object titleResult = getCmd.ExecuteScalar();

                    if (titleResult == null)
                    {
                        Debug.WriteLine("DeleteTask: Task " + taskId + " not found");
                        return false;
                    }

                    string title = titleResult.ToString();

                    // Then delete the task
                    string query = "DELETE FROM Tasks WHERE task_id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", taskId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    Debug.WriteLine("DeleteTask: Task " + taskId + " deleted (Rows: " + rowsAffected + ")");
                    LogAction("Task Deleted", "'" + title + "'");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DeleteTask database error: " + ex.Message);
                return false;
            }
        }

        // ============================================================
        // ACTIVITY LOG METHODS - View logged actions
        // ============================================================

        // Get activity log from memory (last 5-10 actions)
        public string GetActivityLog()
        {
            if (ActivityLog.Count == 0)
                return "No activities recorded yet.";

            int count = Math.Min(10, ActivityLog.Count);
            string result = "Recent actions:\n";
            for (int i = ActivityLog.Count - count; i < ActivityLog.Count; i++)
            {
                result += (i - (ActivityLog.Count - count) + 1) + ". " + ActivityLog[i] + "\n";
            }
            return result;
        }

        // Get full activity log from database
        public string GetFullActivityLog()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT TOP 50 action, details, timestamp FROM ActivityLog ORDER BY timestamp DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    string result = "=== FULL ACTIVITY LOG ===\n";
                    int count = 0;
                    while (reader.Read())
                    {
                        count++;
                        string action = reader.GetString(0);
                        string details = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        DateTime timestamp = reader.GetDateTime(2);
                        result += count + ". " + timestamp.ToShortDateString() + " " + timestamp.ToShortTimeString() +
                                 " - " + action + (string.IsNullOrEmpty(details) ? "" : ": " + details) + "\n";
                    }

                    if (count == 0)
                        return "No activities found in database.";

                    return result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetFullActivityLog database error: " + ex.Message);
                return "Could not retrieve the full activity log.";
            }
        }

        // ============================================================
        // DATABASE VERIFICATION - Check if data is being saved
        // ============================================================

        // Get count of tasks in database (for verification)
        public int GetTaskCount()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Tasks";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                return -1;
            }
        }

        // Get count of activity logs in database (for verification)
        public int GetLogCount()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM ActivityLog";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                return -1;
            }
        }
    }

    // Task item class to represent a single task
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}