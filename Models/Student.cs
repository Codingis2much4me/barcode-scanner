using System;

namespace StudentBarcodeApp.Models
{
    // Simple POCO for one student record. Matches the SQLite schema.
    public class Student
    {
        // Natural key scanned from barcodes.
        public string RollNumber { get; set; } = string.Empty;

        // Basic profile fields shown in the UI.
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string Email     { get; set; } = string.Empty;
        public string Course    { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Year { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;

        // Stored as TEXT in the DB for simplicity.
        public DateTime EnrollmentDate { get; set; }

        public string Status { get; set; } = "Active";

        // File path or pack URI to the student's photo.
        public string PhotoPath { get; set; } = string.Empty;

        // Convenience property for the UI.
        public string FullName => $"{FirstName} {LastName}";

        // Used for the placeholder avatar when PhotoPath is missing.
        public string Initials
        {
            get
            {
                // Build initials from first and last name; fall back to roll number.
                var first = string.IsNullOrWhiteSpace(FirstName) ? string.Empty : FirstName.Trim();
                var last = string.IsNullOrWhiteSpace(LastName) ? string.Empty : LastName.Trim();

                string initials = string.Empty;
                if (!string.IsNullOrEmpty(first)) initials += char.ToUpperInvariant(first[0]);
                if (!string.IsNullOrEmpty(last)) initials += char.ToUpperInvariant(last[0]);
                if (string.IsNullOrEmpty(initials) && !string.IsNullOrWhiteSpace(RollNumber))
                {
                    initials = RollNumber[0].ToString().ToUpperInvariant();
                }
                return initials;
            }
        }
    }
}
