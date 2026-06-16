using DemoExam.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DemoExam.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? imageName = value as string;
            string? fullPath = ImageHelpers.GetFullPath(imageName);
            if (fullPath != null)
                return new BitmapImage(new Uri(fullPath, UriKind.Absolute));

            string? fallback = ImageHelpers.GetFallbackPath();
            return fallback != null ? new BitmapImage(new Uri(fallback, UriKind.Absolute)) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
