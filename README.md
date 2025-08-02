# Student Barcode Scanner Application

A Windows desktop application that reads roll numbers from barcode scanners and displays student information from a local database.

## Features

- **Barcode Scanner Integration**: Automatically detects and processes barcode input from USB barcode scanners
- **Student Database**: SQLite database with comprehensive student information
- **Real-time Search**: Instant student lookup when barcode is scanned
- **Manual Search**: Option to manually enter roll numbers for search
- **Modern UI**: Clean, responsive WPF interface with professional styling
- **Sample Data**: Pre-populated with sample student records for testing

## Requirements

- Windows 10 or later
- .NET 6.0 or later
- USB Barcode Scanner (optional - can use manual input for testing)

## Installation

1. Clone or download the project
2. Open a terminal in the project directory
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the application:
   ```bash
   dotnet build
   ```
5. Run the application:
   ```bash
   dotnet run
   ```

## How to Use

### With Barcode Scanner
1. Connect your USB barcode scanner to the computer
2. Launch the application
3. Scan a barcode containing a student roll number
4. Student information will be displayed automatically

### Manual Entry
1. Launch the application
2. Enter a roll number in the "Manual Search" field
3. Click "Search" or press Enter
4. Student information will be displayed

### Sample Roll Numbers
The application comes with sample data. Try these roll numbers:
- CS001 (John Doe)
- CS002 (Jane Smith)  
- EE001 (Bob Johnson)

## Features

### Scanner Controls
- **Toggle Scanner**: Turn barcode scanning on/off
- **Clear**: Clear the displayed student information
- **Status Indicator**: Visual indicator showing scanner status (green = on, red = off)

### Student Information Display
The application displays comprehensive student information including:
- Full Name
- Roll Number
- Email Address
- Course/Program
- Department
- Academic Year
- Phone Number
- Enrollment Date
- Student Status

## Technical Details

### Architecture
- **WPF (Windows Presentation Foundation)**: Modern Windows desktop UI framework
- **MVVM Pattern**: Clean separation of concerns using Model-View-ViewModel
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection for IoC
- **SQLite Database**: Lightweight, embedded database for student records
- **Async/Await**: Non-blocking database operations

### Database Schema
```sql
CREATE TABLE Students (
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
)
```

### Barcode Scanner Support
The application works with standard USB barcode scanners that emulate keyboard input. Most commercial barcode scanners work out of the box without additional drivers.

## Configuration

### Adding Students
To add more students to the database, you can:
1. Modify the sample data in `DatabaseService.cs`
2. Use the database service methods to programmatically add students
3. Directly modify the SQLite database file located in the application directory

### Customizing Barcode Input
The barcode service can be customized in `BarcodeService.cs` to handle different:
- Barcode formats
- Timeout intervals
- Special character handling

## Troubleshooting

### Barcode Scanner Not Working
1. Ensure the scanner is properly connected
2. Test the scanner in a text editor to verify it's working
3. Check that the scanner is configured for keyboard emulation mode
4. Try toggling the scanner on/off in the application

### Database Issues
1. Check that the application has write permissions in its directory
2. The database file `students.db` will be created automatically
3. Delete the database file to reset with sample data

### Performance
- The application uses async operations for database queries
- Large datasets should perform well due to SQLite's efficiency
- UI remains responsive during database operations

## License

This project is provided as-is for educational and commercial use.
