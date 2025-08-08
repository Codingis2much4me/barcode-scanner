using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Microsoft.Extensions.Logging;
using StudentBarcodeApp.Models;
using System.Threading.Tasks;

namespace StudentBarcodeApp.Services
{
    /// <summary>
    /// Thin wrapper around SQLite for basic CRUD on Students.
    /// Keeps things simple: one connection per operation, parameterized SQL, and minimal schema checks.
    /// </summary>
    public class DatabaseService : IDatabaseService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly string _connectionString;

        /// <summary>
        /// Build a connection string pointing at students.db inside the app folder.
        /// Using the app base directory keeps the DB local to the app and easy to reset.
        /// </summary>
        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "students.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        /// <summary>
        /// Creates the Students table if missing and adds the PhotoPath column if it doesn't exist.
        /// Then seeds a few sample rows the first time (only when the table is empty).
        /// Note: simple "migration" using PRAGMA table_info is enough for this app.
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            // Base schema. EnrollmentDate stored as TEXT (ISO-ish) for simplicity.
            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Students (
                    RollNumber TEXT PRIMARY KEY,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT,
                    Course TEXT,
                    Department TEXT,
                    Year INTEGER,
                    PhoneNumber TEXT,
                    EnrollmentDate TEXT,
                    Status TEXT DEFAULT 'Active',
                    PhotoPath TEXT
                )";
            using var command = new SQLiteCommand(createTableQuery, connection);
            await command.ExecuteNonQueryAsync();

            // Make sure PhotoPath exists (older DBs might not have it).
            try
            {
                // Best-effort schema check using PRAGMA table_info.
                using var pragmaCmd = new SQLiteCommand("PRAGMA table_info(Students)", connection);
                using var reader = await pragmaCmd.ExecuteReaderAsync();
                var hasPhoto = false;
                while (await reader.ReadAsync())
                {
                    if (string.Equals(reader[1]?.ToString(), "PhotoPath", StringComparison.OrdinalIgnoreCase))
                    {
                        hasPhoto = true;
                        break;
                    }
                }
                if (!hasPhoto)
                {
                    using var alter = new SQLiteCommand("ALTER TABLE Students ADD COLUMN PhotoPath TEXT", connection);
                    await alter.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Non-fatal: log and continue.
                _logger.LogWarning(ex, "PhotoPath check failed; continuing");
            }

            // Seed a few rows the first time so the app is usable right away.
            await AddSampleDataIfEmptyAsync(connection);
            _logger.LogInformation("Database initialized");
        }

        /// <summary>
        /// Inserts a few demo records when the table is empty.
        /// Useful for local testing without having to type data manually.
        /// </summary>
        private async Task AddSampleDataIfEmptyAsync(SQLiteConnection connection)
        {
            // If there is already data, don't touch it.
            using var countCommand = new SQLiteCommand("SELECT COUNT(*) FROM Students", connection);
            var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
            if (count != 0) return;

            var sampleStudents = new[]
            {
                // Simple demo rows. PhotoPath uses site-of-origin so images can sit next to the exe.
                new Student
                {
                    RollNumber = "CS001",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@university.edu",
                    Course = "Computer Science",
                    Department = "Engineering",
                    Year = 2,
                    PhoneNumber = "+1-555-0123",
                    // Store as TEXT (formatted below when writing)
                    EnrollmentDate = DateTime.Now.AddYears(-2),
                    Status = "Active",
                    // Using pack site-of-origin so images can live next to the exe in Resources/Images
                    PhotoPath = "pack://siteoforigin:,,,/Resources/Images/CS001.png"
                },
                new Student
                {
                    RollNumber = "CS002",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@university.edu",
                    Course = "Computer Science",
                    Department = "Engineering",
                    Year = 3,
                    PhoneNumber = "+1-555-0124",
                    EnrollmentDate = DateTime.Now.AddYears(-3),
                    Status = "Active",
                    PhotoPath = "pack://siteoforigin:,,,/Resources/Images/CS002.png"
                },
                new Student
                {
                    RollNumber = "EE001",
                    FirstName = "Bob",
                    LastName = "Johnson",
                    Email = "bob.johnson@university.edu",
                    Course = "Electrical Engineering",
                    Department = "Engineering",
                    Year = 1,
                    PhoneNumber = "+1-555-0125",
                    EnrollmentDate = DateTime.Now.AddMonths(-6),
                    Status = "Active",
                    PhotoPath = "pack://siteoforigin:,,,/Resources/Images/EE001.png"
                }
            };

            foreach (var s in sampleStudents)
                await AddStudentInternalAsync(connection, s);

            _logger.LogInformation("Sample data inserted");
        }

        /// <summary>
        /// Finds a single student by roll number.
        /// Uses parameters to avoid SQL injection and to keep types consistent.
        /// </summary>
        public async Task<Student?> GetStudentByRollNumberAsync(string rollNumber)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM Students WHERE RollNumber = @rollNumber";
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@rollNumber", rollNumber);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                // Defensive ToString/null checks so bad data doesn't crash the UI.
                return new Student
                {
                    RollNumber = reader["RollNumber"].ToString() ?? "",
                    FirstName = reader["FirstName"].ToString() ?? "",
                    LastName = reader["LastName"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    Course = reader["Course"].ToString() ?? "",
                    Department = reader["Department"].ToString() ?? "",
                    Year = Convert.ToInt32(reader["Year"]),
                    PhoneNumber = reader["PhoneNumber"].ToString() ?? "",
                    // EnrollmentDate stored as TEXT. If it's missing/bad, fallback to Now.
                    EnrollmentDate = DateTime.Parse(reader["EnrollmentDate"].ToString() ?? DateTime.Now.ToString()),
                    Status = reader["Status"].ToString() ?? "Active",
                    PhotoPath = reader["PhotoPath"].ToString() ?? string.Empty
                };
            }
            return null;
        }

        /// <summary>
        /// Returns all students sorted by roll number.
        /// Fine for small datasets like this (single table, local app).
        /// </summary>
        public async Task<List<Student>> GetAllStudentsAsync()
        {
            var students = new List<Student>();
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SQLiteCommand("SELECT * FROM Students ORDER BY RollNumber", connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                students.Add(new Student
                {
                    RollNumber = reader["RollNumber"].ToString() ?? "",
                    FirstName = reader["FirstName"].ToString() ?? "",
                    LastName = reader["LastName"].ToString() ?? "",
                    Email = reader["Email"].ToString() ?? "",
                    Course = reader["Course"].ToString() ?? "",
                    Department = reader["Department"].ToString() ?? "",
                    Year = Convert.ToInt32(reader["Year"]),
                    PhoneNumber = reader["PhoneNumber"].ToString() ?? "",
                    EnrollmentDate = DateTime.Parse(reader["EnrollmentDate"].ToString() ?? DateTime.Now.ToString()),
                    Status = reader["Status"].ToString() ?? "Active",
                    PhotoPath = reader["PhotoPath"].ToString() ?? string.Empty
                });
            }

            return students;
        }

        /// <summary>
        /// Adds a student (opens its own connection and reuses the same insert logic).
        /// </summary>
        public async Task AddStudentAsync(Student student)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            await AddStudentInternalAsync(connection, student);
        }

        /// <summary>
        /// Internal helper so seeding can reuse the same insert command on an existing open connection.
        /// </summary>
        private static async Task AddStudentInternalAsync(SQLiteConnection connection, Student student)
        {
            // Dates stored as TEXT ("yyyy-MM-dd HH:mm:ss") for easy sorting/display.
            var query = @"
                INSERT INTO Students (RollNumber, FirstName, LastName, Email, Course, Department, Year, PhoneNumber, EnrollmentDate, Status, PhotoPath)
                VALUES (@rollNumber, @firstName, @lastName, @email, @course, @department, @year, @phoneNumber, @enrollmentDate, @status, @photoPath)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@rollNumber", student.RollNumber);
            command.Parameters.AddWithValue("@firstName", student.FirstName);
            command.Parameters.AddWithValue("@lastName", student.LastName);
            command.Parameters.AddWithValue("@email", student.Email);
            command.Parameters.AddWithValue("@course", student.Course);
            command.Parameters.AddWithValue("@department", student.Department);
            command.Parameters.AddWithValue("@year", student.Year);
            command.Parameters.AddWithValue("@phoneNumber", student.PhoneNumber);
            // Store dates as "yyyy-MM-dd HH:mm:ss" TEXT so they sort lexicographically and are easy to display.
            command.Parameters.AddWithValue("@enrollmentDate", student.EnrollmentDate.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@status", student.Status);
            command.Parameters.AddWithValue("@photoPath", string.IsNullOrWhiteSpace(student.PhotoPath) ? (object)DBNull.Value : student.PhotoPath);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Updates all editable fields for a student identified by RollNumber.
        /// </summary>
        public async Task UpdateStudentAsync(Student student)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE Students 
                SET FirstName = @firstName, LastName = @lastName, Email = @email, 
                    Course = @course, Department = @department, Year = @year, 
                    PhoneNumber = @phoneNumber, EnrollmentDate = @enrollmentDate, Status = @status,
                    PhotoPath = @photoPath
                WHERE RollNumber = @rollNumber";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@rollNumber", student.RollNumber);
            command.Parameters.AddWithValue("@firstName", student.FirstName);
            command.Parameters.AddWithValue("@lastName", student.LastName);
            command.Parameters.AddWithValue("@email", student.Email);
            command.Parameters.AddWithValue("@course", student.Course);
            command.Parameters.AddWithValue("@department", student.Department);
            command.Parameters.AddWithValue("@year", student.Year);
            command.Parameters.AddWithValue("@phoneNumber", student.PhoneNumber);
            command.Parameters.AddWithValue("@enrollmentDate", student.EnrollmentDate.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@status", student.Status);
            command.Parameters.AddWithValue("@photoPath", string.IsNullOrWhiteSpace(student.PhotoPath) ? (object)DBNull.Value : student.PhotoPath);

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Deletes a student by roll number.
        /// </summary>
        public async Task DeleteStudentAsync(string rollNumber)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SQLiteCommand("DELETE FROM Students WHERE RollNumber = @rollNumber", connection);
            command.Parameters.AddWithValue("@rollNumber", rollNumber);
            await command.ExecuteNonQueryAsync();
        }
    }
}
