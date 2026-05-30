using System.Globalization;

namespace ExpenseTracker.Converters
{
    public class ProgressToPercentStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double d ? $"{Math.Round(d * 100)}%" : "0%";

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
