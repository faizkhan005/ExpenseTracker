using System.Globalization;

namespace ExpenseTracker.Converters
{
    public class BoolToVisibilityConverter :IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && b;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is bool b && b;
    }
}
