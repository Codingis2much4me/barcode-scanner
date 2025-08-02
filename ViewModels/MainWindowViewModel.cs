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
    public class MainWindowViewModel : INotifyPropertyChanged
    {
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
            get => _currentStudent;
            set
            {
                _currentStudent = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsScanning
        {
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
            _databaseService = databaseService;
            _barcodeService = barcodeService;
            _logger = logger;

            ToggleScanningCommand = new RelayCommand(ToggleScanning);
            SearchManualCommand = new RelayCommand(SearchManual);
            ClearCommand = new RelayCommand(Clear);

            _barcodeService.BarcodeScanned += OnBarcodeScanned;
            
            // Initialize database and start scanning
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
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
            await SearchForStudent(rollNumber);
        }

        private async Task SearchForStudent(string rollNumber)
        {
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
            if (!string.IsNullOrWhiteSpace(ManualRollNumber))
            {
                await SearchForStudent(ManualRollNumber.Trim());
            }
        }

        private void Clear()
        {
            CurrentStudent = null;
            ManualRollNumber = string.Empty;
            StatusMessage = IsScanning ? "Ready to scan barcode..." : "Barcode scanning stopped.";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

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
