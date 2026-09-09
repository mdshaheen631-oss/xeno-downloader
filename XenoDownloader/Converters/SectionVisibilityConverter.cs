using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace XenoDownloader.Converters
{
    public class SectionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNullOrEmpty = value == null ||
                (value is string s && string.IsNullOrWhiteSpace(s)) ||
                (value is bool b && !b) ||
                (value is int i && i == 0);

            if (Invert) isNullOrEmpty = !isNullOrEmpty;

            return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
