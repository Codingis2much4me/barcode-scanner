using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using StudentBarcodeApp.Services;
using StudentBarcodeApp.ViewModels;

namespace StudentBarcodeApp
{
    public partial class MainWindow : Window
    {
        private readonly BarcodeService _barcodeService;

        public MainWindow(MainWindowViewModel viewModel, IBarcodeService barcodeService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _barcodeService = (BarcodeService)barcodeService;
            
            // Handle global key events for barcode scanning
            this.KeyDown += MainWindow_KeyDown;
            this.Focusable = true;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Process key input for barcode scanning
            _barcodeService.ProcessKeyInput(e.Key);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            // Ensure the window can receive key events
            this.Focus();
        }
    }

    // Converter for null to boolean conversion
    public class NullToBooleanConverter : IValueConverter
    {
        public static readonly NullToBooleanConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
