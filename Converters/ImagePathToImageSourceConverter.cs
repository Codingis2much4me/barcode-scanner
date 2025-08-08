// Converts a file path or pack URI into a BitmapImage for Image.Source.
// Handles pack://, relative/absolute paths, and freezes the bitmap to be UI-thread safe.
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace StudentBarcodeApp.Converters
{
    public sealed class ImagePathToImageSourceConverter : IValueConverter
    {
        // value: string path. Returns BitmapImage or Binding.DoNothing on failure.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path)) return Binding.DoNothing;

            try
            {
                // Build a URI from either a pack URI or a filesystem path.
                Uri uri;
                if (path.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                {
                    uri = new Uri(path, UriKind.Absolute);
                }
                else
                {
                    var absolutePath = Path.IsPathRooted(path)
                        ? path
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                    if (!File.Exists(absolutePath))
                        return Binding.DoNothing;

                    uri = new Uri(absolutePath, UriKind.Absolute);
                }

                // Load the image and close the file handle right away.
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad; // OnLoad -> no locked files
                bmp.UriSource = uri;
                bmp.EndInit();
                if (bmp.CanFreeze) bmp.Freeze(); // safe to use across threads
                return bmp;
            }
            catch
            {
                // Failing quietly here lets the UI fall back to a placeholder.
                return Binding.DoNothing;
            }
        }

        // One-way binding only; not needed in this app.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}