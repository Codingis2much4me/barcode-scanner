using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using StudentBarcodeApp.Services;
using StudentBarcodeApp.ViewModels;

namespace StudentBarcodeApp
{
    // Main window: delegates work to the view model and barcode service. Code-behind stays minimal.
    public partial class MainWindow : Window
    {
        // Barcode service processes keyboard-emulated scanner input.
        private readonly IBarcodeService _barcodeService;

        public MainWindow(MainWindowViewModel viewModel, IBarcodeService barcodeService)
        {
            // Wire up generated XAML partial and bind to the view model.
            InitializeComponent();
            DataContext = viewModel;
            _barcodeService = barcodeService;

            // Listen at window level so focused controls don't swallow scanner keys.
            KeyDown += MainWindow_KeyDown;
            Focusable = true;
        }

        private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            // Forward raw keys; the service buffers until Enter and then raises BarcodeScanned.
            _barcodeService.ProcessKeyInput(e.Key);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            // Make sure the window can receive keyboard input when brought to front.
            Focus();
        }
    }

    // Quick helper used by XAML triggers to check for null.
    public class NullToBooleanConverter : IValueConverter
    {
        public static readonly NullToBooleanConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value != null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
