using System;
using System.Globalization;
using System.Windows.Data;

namespace StudentBarcodeApp.Converters
{
    // Small helper converters used by XAML bindings.
    // Keeping them in a single file avoids clutter.
    public class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}