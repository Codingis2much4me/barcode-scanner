using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Microsoft.Extensions.Logging;
using StudentBarcodeApp.Models;
using System.Threading.Tasks;

namespace StudentBarcodeApp.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly ILogger<DatabaseService> _logger;
        private readonly string _connectionString;

        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "students.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

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
                        Status TEXT DEFAULT 'Active'
                    )";

                using var command = new SQLiteCommand(createTableQuery, connection);
                await command.ExecuteNonQueryAsync();

                // Add sample data if table is empty
                await AddSampleDataIfEmptyAsync(connection);

                _logger.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
                throw;
            }
        }

        private async Task AddSampleDataIfEmptyAsync(SQLiteConnection connection)
        {
            var countQuery = "SELECT COUNT(*) FROM Students";
            using var countCommand = new SQLiteCommand(countQuery, connection);
            var count = Convert.ToInt32(await countCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                var sampleStudents = new[]
                {
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
                        EnrollmentDate = DateTime.Now.AddYears(-2),
                        Status = "Active"
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
                        Status = "Active"
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
                        Status = "Active"
                    }
                };

                foreach (var student in sampleStudents)
                {
                    await AddStudentInternalAsync(connection, student);
                }

                _logger.LogInformation("Sample data added to database");
            }
        }

        public async Task<Student?> GetStudentByRollNumberAsync(string rollNumber)
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Students WHERE RollNumber = @rollNumber";
                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@rollNumber", rollNumber);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
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
                        EnrollmentDate = DateTime.Parse(reader["EnrollmentDate"].ToString() ?? DateTime.Now.ToString()),
                        Status = reader["Status"].ToString() ?? "Active"
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student with roll number {RollNumber}", rollNumber);
                throw;
            }
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            try
            {
                var students = new List<Student>();
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT * FROM Students ORDER BY RollNumber";
                using var command = new SQLiteCommand(query, connection);
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
                        Status = reader["Status"].ToString() ?? "Active"
                    });
                }

                return students;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all students");
                throw;
            }
        }

        public async Task AddStudentAsync(Student student)
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();
                await AddStudentInternalAsync(connection, student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding student {RollNumber}", student.RollNumber);
                throw;
            }
        }

        private async Task AddStudentInternalAsync(SQLiteConnection connection, Student student)
        {
            var query = @"
                INSERT INTO Students (RollNumber, FirstName, LastName, Email, Course, Department, Year, PhoneNumber, EnrollmentDate, Status)
                VALUES (@rollNumber, @firstName, @lastName, @email, @course, @department, @year, @phoneNumber, @enrollmentDate, @status)";

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

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateStudentAsync(Student student)
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    UPDATE Students 
                    SET FirstName = @firstName, LastName = @lastName, Email = @email, 
                        Course = @course, Department = @department, Year = @year, 
                        PhoneNumber = @phoneNumber, EnrollmentDate = @enrollmentDate, Status = @status
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

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {RollNumber}", student.RollNumber);
                throw;
            }
        }

        public async Task DeleteStudentAsync(string rollNumber)
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                await connection.OpenAsync();

                var query = "DELETE FROM Students WHERE RollNumber = @rollNumber";
                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@rollNumber", rollNumber);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {RollNumber}", rollNumber);
                throw;
            }
        }
    }
}
