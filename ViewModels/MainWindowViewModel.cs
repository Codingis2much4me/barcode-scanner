using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using StudentBarcodeApp.Models;
using StudentBarcodeApp.Services;

namespace StudentBarcodeApp.ViewModels
{
    // ViewModel for the main window. Owns UI state and coordinates services.
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // Services are injected to keep code-behind thin and testable.
        private readonly IDatabaseService _databaseService;
        private readonly IBarcodeService _barcodeService;
        private readonly ILogger<MainWindowViewModel> _logger;

        private Student? _currentStudent;
        private string _statusMessage = "Ready to scan barcode...";
        private bool _isScanning = true;
        private string _manualRollNumber = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Student? CurrentStudent
        {
            // Notify UI when the selected student changes.
            get => _currentStudent;
            set
            {
                _currentStudent = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            // Short status text shown in the footer.
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsScanning
        {
            // Backed by the service; also updates ScanningStatusText.
            get => _isScanning;
            set
            {
                _isScanning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScanningStatusText));
            }
        }

        public string ScanningStatusText => IsScanning ? "Scanner: ON" : "Scanner: OFF";

        public string ManualRollNumber
        {
            // Two-way bound to the manual search TextBox.
            get => _manualRollNumber;
            set
            {
                _manualRollNumber = value;
                OnPropertyChanged();
            }
        }

        public ICommand ToggleScanningCommand { get; }
        public ICommand SearchManualCommand { get; }
        public ICommand ClearCommand { get; }

        public MainWindowViewModel(
            IDatabaseService databaseService,
            IBarcodeService barcodeService,
            ILogger<MainWindowViewModel> logger)
        {
            // Store dependencies and wire up commands/events.
            _databaseService = databaseService;
            _barcodeService = barcodeService;
            _logger = logger;

            ToggleScanningCommand = new RelayCommand(ToggleScanning);
            SearchManualCommand = new RelayCommand(SearchManual);
            ClearCommand = new RelayCommand(Clear);

            _barcodeService.BarcodeScanned += OnBarcodeScanned;
            
            // Initialize database and start listening so the app is usable right away.
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            // Create DB file if missing, apply simple schema checks, then start scanner.
            try
            {
                await _databaseService.InitializeDatabaseAsync();
                _barcodeService.StartListening();
                StatusMessage = "Ready to scan barcode...";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing application");
                StatusMessage = "Error initializing application. Please restart.";
            }
        }

        private async void OnBarcodeScanned(string rollNumber)
        {
            // Event from the barcode service -> run a lookup.
            await SearchForStudent(rollNumber);
        }

        private async Task SearchForStudent(string rollNumber)
        {
            // Show progress in the status bar, then update CurrentStudent (or clear if not found).
            try
            {
                StatusMessage = $"Searching for student: {rollNumber}...";
                
                var student = await _databaseService.GetStudentByRollNumberAsync(rollNumber);
                
                if (student != null)
                {
                    CurrentStudent = student;
                    StatusMessage = $"Student found: {student.FullName}";
                    _logger.LogInformation("Student found: {RollNumber} - {FullName}", rollNumber, student.FullName);
                }
                else
                {
                    CurrentStudent = null;
                    StatusMessage = $"No student found with roll number: {rollNumber}";
                    _logger.LogWarning("No student found with roll number: {RollNumber}", rollNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for student with roll number: {RollNumber}", rollNumber);
                StatusMessage = "Error occurred while searching for student.";
                CurrentStudent = null;
            }
        }

        private void ToggleScanning()
        {
            // Flip the service state and update UI text accordingly.
            if (IsScanning)
            {
                _barcodeService.StopListening();
                IsScanning = false;
                StatusMessage = "Barcode scanning stopped.";
            }
            else
            {
                _barcodeService.StartListening();
                IsScanning = true;
                StatusMessage = "Ready to scan barcode...";
            }
        }

        private async void SearchManual()
        {
            // Manual lookup from the TextBox, trimming obvious whitespace.
            if (!string.IsNullOrWhiteSpace(ManualRollNumber))
            {
                await SearchForStudent(ManualRollNumber.Trim());
            }
        }

        private void Clear()
        {
            // Reset UI to a friendly default.
            CurrentStudent = null;
            ManualRollNumber = string.Empty;
            StatusMessage = IsScanning ? "Ready to scan barcode..." : "Barcode scanning stopped.";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            // Standard INotifyPropertyChanged pattern.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Simple ICommand wrapper for button clicks.
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }
}
