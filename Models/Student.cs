using System;

namespace StudentBarcodeApp.Models
{
    public class Student
    {
        public string RollNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Year { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = "Active";

        public string FullName => $"{FirstName} {LastName}";
    }
}
